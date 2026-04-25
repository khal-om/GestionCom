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
        }

        public async Task UpdatePaymentReceivedStatusAsync(int paymentId, UpdatePaymentStatusDto dto, int userId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new InvalidOperationException("Paiement introuvable.");

            payment.IsReceived = dto.IsReceived;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = userId;

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
