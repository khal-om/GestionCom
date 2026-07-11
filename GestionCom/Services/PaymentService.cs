using GestionCom.Data;
using GestionCom.DTOs.Payment;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAsync(CreatePaymentDto dto, int userId)
        {
            if (dto.Amount <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            if (string.IsNullOrWhiteSpace(dto.PaymentMode))
                throw new Exception("Le mode de paiement est requis.");

            var isDeferredPayment = dto.PaymentMode is PaymentModeOption.Cheque or PaymentModeOption.Traite;

            if (isDeferredPayment)
            {
                if (string.IsNullOrWhiteSpace(dto.PaymentNumber))
                    throw new Exception("Le numéro du règlement est requis.");

                if (!dto.PaymentDate.HasValue)
                    throw new Exception("La date règlement est requise.");

                if (!dto.DueDate.HasValue)
                    throw new Exception("La date d'échéance est requise.");
            }

            var payment = new Payment
            {
                ClientId = dto.ClientId,
                VenteId = dto.VenteId,
                Amount = dto.Amount,
                PaymentMode = dto.PaymentMode,
                PaymentNumber = Normalize(dto.PaymentNumber),
                PaymentDate = NormalizeToUtc(dto.PaymentDate),
                DueDate = NormalizeToUtc(dto.DueDate),
                IsReceived = !isDeferredPayment || dto.IsReceived,
                Note = BuildPaymentNote(dto),
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            // Server-side validation: if payment is assigned to a specific vente, ensure it does not exceed remaining amount
            if (payment.VenteId.HasValue)
            {
                var venteForCheck = await _context.Ventes.FirstOrDefaultAsync(v => v.Id == payment.VenteId.Value);
                if (venteForCheck == null)
                    throw new InvalidOperationException("Vente introuvable.");

                var totalTtc = venteForCheck.TotalAmount * 1.19m;
                // Recompute paid sum from received payments to avoid stale vente.PaidAmount
                var paidSum = await _context.Payments
                    .Where(p => p.VenteId == venteForCheck.Id && p.IsReceived)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                var remaining = totalTtc - paidSum;
                var remainingRounded = decimal.Round(remaining, 3, MidpointRounding.AwayFromZero);
                var amountRounded = decimal.Round(payment.Amount, 3, MidpointRounding.AwayFromZero);

                if (remainingRounded <= 0)
                    throw new InvalidOperationException("Cette vente est déjà totalement payée.");

                // Allow exact equality within rounding precision; reject only if amount is strictly greater
                if (amountRounded > remainingRounded + 0.0000001m)
                    throw new InvalidOperationException($"Le montant ({amountRounded:N3}) dépasse le reste dû pour cette vente ({remainingRounded:N3}).");
            }

            if (isDeferredPayment && !payment.IsReceived && payment.DueDate.HasValue)
            {
                var clientName = await _context.Clients
                    .AsNoTracking()
                    .Where(c => c.Id == dto.ClientId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync() ?? "Client";

                var calendarEvent = new CalendarEvent
                {
                    EventDate = payment.DueDate.Value,
                    Description = $"Échéance {payment.PaymentMode.ToLowerInvariant()} {payment.PaymentNumber} - {clientName} - {dto.Amount:N2}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.CalendarEvents.Add(calendarEvent);
                await _context.SaveChangesAsync();
                payment.CalendarEventId = calendarEvent.Id;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // If this payment applies to a specific vente and is received, update the vente paid amount and status
            if (payment.IsReceived && payment.VenteId.HasValue)
            {
                var vente = await _context.Ventes.FirstOrDefaultAsync(v => v.Id == payment.VenteId.Value);
                if (vente != null)
                {
                    // Recompute paid sum from received payments (including this new one)
                    var paidSum = await _context.Payments
                        .Where(p => p.VenteId == vente.Id && p.IsReceived)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    // Ensure PaidAmount stored on the vente matches the computed sum
                    vente.PaidAmount = paidSum;

                    var totalTtc = vente.TotalAmount * 1.19m;

                    if (vente.PaidAmount >= decimal.Round(totalTtc, 3, MidpointRounding.AwayFromZero))
                    {
                        vente.PaiementEtat = PaiementEtat.Payee;
                    }
                    else if (vente.PaidAmount > 0)
                    {
                        vente.PaiementEtat = PaiementEtat.EnCours;
                    }
                    else
                    {
                        vente.PaiementEtat = PaiementEtat.NonPayee;
                    }

                    vente.UpdatedAt = DateTime.UtcNow;
                    vente.UpdatedBy = userId;

                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
        }

        public async Task UpdatePaymentReceivedStatusAsync(int paymentId, UpdatePaymentStatusDto dto, int userId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new InvalidOperationException("Paiement introuvable.");

            var previous = payment.IsReceived;

            payment.IsReceived = dto.IsReceived;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = userId;

            // If the payment is linked to a vente, adjust that vente's PaidAmount when received status changes
            if (payment.VenteId.HasValue)
            {
                var vente = await _context.Ventes.FirstOrDefaultAsync(v => v.Id == payment.VenteId.Value);
                if (vente != null)
                {
                    // Recompute paid sum from received payments to avoid incremental drift
                    var paidSum = await _context.Payments
                        .Where(p => p.VenteId == vente.Id && p.IsReceived)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    vente.PaidAmount = paidSum;

                    var totalTtc = decimal.Round(vente.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero);
                    var paidRounded = decimal.Round(vente.PaidAmount, 3, MidpointRounding.AwayFromZero);

                    if (paidRounded >= totalTtc)
                    {
                        vente.PaiementEtat = PaiementEtat.Payee;
                    }
                    else if (paidRounded > 0)
                    {
                        vente.PaiementEtat = PaiementEtat.EnCours;
                    }
                    else
                    {
                        vente.PaiementEtat = PaiementEtat.NonPayee;
                    }

                    vente.UpdatedAt = DateTime.UtcNow;
                    vente.UpdatedBy = userId;
                }
            }

            if (payment.IsReceived && payment.CalendarEventId.HasValue)
            {
                var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == payment.CalendarEventId.Value);
                if (calendarEvent != null)
                {
                    _context.CalendarEvents.Remove(calendarEvent);
                }

                payment.CalendarEventId = null;
            }

            await _context.SaveChangesAsync();
        }

        private static string? BuildPaymentNote(CreatePaymentDto dto)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(dto.Label))
            {
                parts.Add($"Libellé: {dto.Label.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                parts.Add(dto.Note.Trim());
            }

            return parts.Count == 0 ? null : string.Join(" | ", parts);
        }

        private static DateTime? NormalizeToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.Kind == DateTimeKind.Utc
                ? value.Value
                : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
