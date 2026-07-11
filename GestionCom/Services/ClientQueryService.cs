using GestionCom.DTOs.Client;
using Microsoft.EntityFrameworkCore;
using GestionCom.Data;

namespace GestionCom.Services
{
    public class ClientQueryService
    {
        private readonly AppDbContext _context;

        public ClientQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClientListDto>> GetClientsAsync()
        {
            var clients = await _context.Clients
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .Select(c => new ClientListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Ville = c.Ville,
                    Telephone = c.Telephone,

                    // Debt computed on TTC amounts (TotalAmount + TVA 19%)
                    Debt = (c.Ventes.Sum(v => (decimal?)(v.TotalAmount * 1.19m)) ?? 0)
                        - (c.Payments.Where(p => p.IsReceived).Sum(p => (decimal?)p.Amount) ?? 0)
                })
                .ToListAsync();

            return clients;
        }

        public async Task<ClientDetailDto> GetClientDetailAsync(int clientId)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .Where(c => c.Id == clientId)
                .Select(c => new ClientDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Profession = c.Profession,
                    MatriculeFiscale = c.MatriculeFiscale,
                    Email = c.Email,
                    Telephone = c.Telephone,
                    Portable = c.Portable,
                    Fax = c.Fax,
                    Adresse = c.Adresse,
                    CodePostal = c.CodePostal,
                    Ville = c.Ville,
                    Pays = c.Pays,
                    Banque = c.Banque,
                    RIB = c.RIB,
                    Note = c.Note,
                    // TotalSpent reported TTC
                    TotalSpent = c.Ventes.Sum(v => (decimal?)(v.TotalAmount * 1.19m)) ?? 0,
                    TotalPaid = c.Payments.Where(p => p.IsReceived).Sum(p => (decimal?)p.Amount) ?? 0
                })
                .FirstOrDefaultAsync();

            if (client == null)
                throw new InvalidOperationException("Client introuvable.");

            var ventes = await _context.Ventes
                .AsNoTracking()
                .Where(v => v.ClientId == clientId)
                .Select(v => new ClientLedgerItemDto
                {
                    Date = v.DateVente,
                    Objet = $"Vente {v.Id}",
                    Type = "Vente",
                    ReferenceId = v.Id,
                    // Debit shown TTC
                    Debit = v.TotalAmount * 1.19m,
                    Credit = 0
                })
                .ToListAsync();

            var ventesList = await _context.Ventes
                .AsNoTracking()
                .Where(v => v.ClientId == clientId)
                .Select(v => new ClientVenteItemDto
                {
                    Id = v.Id,
                    DocumentNumber = v.DocumentNumber,
                    DateVente = v.DateVente,
                    // Compute amounts using payments sum and apply rounding to 3 decimals to avoid precision mismatches
                    TotalAmount = decimal.Round(v.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero),
                    PaidAmount = decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero),
                    PaymentStatus = (
                        decimal.Round((v.TotalAmount * 1.19m), 3, MidpointRounding.AwayFromZero)
                        - decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero)
                        <= 0)
                        ? "Payée"
                        : ((decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero) > 0) ? "En cours" : "Non payée")
                })
                .OrderByDescending(v => v.DateVente)
                .ToListAsync();

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ClientPaymentItemDto
                {
                    Id = p.Id,
                    Date = p.CreatedAt,
                    Amount = p.Amount,
                    PaymentMode = p.PaymentMode,
                    PaymentNumber = p.PaymentNumber,
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    Label = p.Note,
                    IsReceived = p.IsReceived,
                    Note = p.Note,
                    VenteId = p.VenteId,
                    VenteRef = p.Vente != null ? (p.Vente.DocumentNumber + " — " + (p.Vente.TotalAmount * 1.19m).ToString("N3")) : null
                })
                .ToListAsync();

            client.Debt = client.TotalSpent - client.TotalPaid;
            client.Payments = payments;
            client.Ventes = ventesList;
            client.Ledger = ventes
                .Concat(payments
                    .Where(p => p.IsReceived)
                    .Select(p => new ClientLedgerItemDto
                {
                    Date = p.Date,
                    Objet = $"Paiement ({p.PaymentMode})",
                    Type = "Paiement",
                    ReferenceId = p.Id,
                    Debit = 0,
                    Credit = p.Amount
                }))
                .OrderBy(l => l.Date)
                .ThenBy(l => l.Objet)
                .ToList();

            var balance = 0m;

            foreach (var item in client.Ledger)
            {
                balance += item.Debit - item.Credit;
                item.Balance = balance;
            }

            return client;
        }
    }
}
