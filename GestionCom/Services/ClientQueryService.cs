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

                    Debt = (c.Ventes.Sum(v => (decimal?)v.TotalAmount) ?? 0)
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
                    TotalSpent = c.Ventes.Sum(v => (decimal?)v.TotalAmount) ?? 0,
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
                    Debit = v.TotalAmount,
                    Credit = 0
                })
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
                    Note = p.Note
                })
                .ToListAsync();

            client.Debt = client.TotalSpent - client.TotalPaid;
            client.Payments = payments;
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
