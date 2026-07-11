using GestionCom.Data;
using GestionCom.DTOs.Vente;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GestionCom.Services
{
    public class VenteQueryService
    {
        private readonly AppDbContext _context;

        public VenteQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenteListItemDto>> GetVentesAsync(int userId, bool isAdmin = false)
        {
            var query = _context.Ventes
                .AsNoTracking()
                .Where(v => isAdmin || v.CreatedBy == userId)
                .OrderByDescending(v => v.DateVente)
                .ThenByDescending(v => v.Id)
                .Select(v => new VenteListItemDto
                {
                    Id = v.Id,
                    ClientId = v.ClientId,
                    ClientName = v.Client.Name,
                    DateVente = v.DateVente,
                    // Present TotalAmount including TVA (19%) and use rounding to avoid precision issues
                    TotalAmount = decimal.Round(v.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero),
                    PaidAmount = decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero),
                    Remaining = decimal.Round(decimal.Round(v.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero) - decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero), 3, MidpointRounding.AwayFromZero),
                    PaymentStatus = (
                        decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero)
                        >= decimal.Round(v.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero)) ? "Payée" : ((decimal.Round((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero) > 0) ? "En cours" : "Non payée"),
                    Etat = v.Etat == VenteEtat.Livre ? "Livrée" : "Commande",
                    LineCount = v.Lignes.Count
                });

            return await query.ToListAsync();
        }

        public async Task<VenteDetailDto> GetVenteDetailAsync(int venteId, int userId, bool isAdmin = false)
        {
            var vente = await _context.Ventes
                .AsNoTracking()
                .Where(v => v.Id == venteId && (isAdmin || v.CreatedBy == userId))
                .Select(v => new VenteDetailDto
                {
                    Id = v.Id,
                    DocumentNumber = v.DocumentNumber,
                    FactureNumber = v.FactureNumber,
                    ClientId = v.ClientId,
                    ClientName = v.Client.Name,
                    ClientEmail = v.Client.Email,
                    ClientTelephone = v.Client.Telephone,
                    ClientPortable = v.Client.Portable,
                    ClientMatriculeFiscale = v.Client.MatriculeFiscale,
                    ClientAddress = v.Client.Adresse,
                    ClientPostalCode = v.Client.CodePostal,
                    ClientCity = v.Client.Ville,
                    ClientCountry = v.Client.Pays,
                    DateVente = v.DateVente,
                    DateLivraison = v.DateLivraison,
                    // Present TotalAmount including TVA (19%)
                    TotalAmount = decimal.Round(v.TotalAmount * 1.19m, 3, MidpointRounding.AwayFromZero),
                    Remaining = decimal.Round((v.TotalAmount * 1.19m) - (_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount), 3, MidpointRounding.AwayFromZero),
                    PaidAmount = (_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount),
                    PaymentStatus = (((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount)) >= (v.TotalAmount * 1.19m)) ? "Payée" : (((_context.Payments.Where(p => p.VenteId == v.Id && p.IsReceived).Sum(p => (decimal?)p.Amount) ?? v.PaidAmount) > 0) ? "En cours" : "Non payée"),
                    Etat = v.Etat == VenteEtat.Livre ? "Livrée" : "Commande",
                    Note = v.Note,
                    Lignes = v.Lignes
                        .OrderBy(l => l.Id)
                        .Select(l => new VenteDetailLineDto
                        {
                            ProduitId = l.ProduitId,
                            ProduitLabel = string.IsNullOrWhiteSpace(l.Produit.Marque)
                                ? l.Produit.Designation
                                : l.Produit.Marque + " - " + l.Produit.Designation,
                            Quantite = l.Quantite,
                            PrixUnitaire = l.PrixUnitaire,
                            RemisePercent = l.RemisePercent,
                            PrixUnitaireApresRemise = l.PrixUnitaire.HasValue
                                ? decimal.Round(l.PrixUnitaire.Value * (1 - (l.RemisePercent / 100m)), 3, MidpointRounding.AwayFromZero)
                                : null,
                            TotalLigne = l.PrixUnitaire.HasValue
                                ? decimal.Round(l.PrixUnitaire.Value * (1 - (l.RemisePercent / 100m)) * l.Quantite, 3, MidpointRounding.AwayFromZero)
                                : null,
                            CurrentStock = l.Produit.StockMovements.Sum(m => (int?)m.Quantity) ?? 0
                        })
                        .ToList()
                    ,
                    // Payments linked to this vente
                    // We'll load payments separately below because projection with collections is limited
                })
                .FirstOrDefaultAsync();

            if (vente == null)
                throw new InvalidOperationException("Vente introuvable.");

            // Load payments for this vente
            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.VenteId == venteId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new GestionCom.DTOs.Vente.VentePaymentItemDto
                {
                    Id = p.Id,
                    Date = p.CreatedAt,
                    Amount = p.Amount,
                    PaymentMode = p.PaymentMode,
                    PaymentNumber = p.PaymentNumber,
                    IsReceived = p.IsReceived,
                    Note = p.Note,
                    ClientId = p.ClientId
                })
                .ToListAsync();

            // attach to dto via dynamic property: we'll add a Payments property to VenteDetailDto
            // but to avoid modifying many files instantly, set via reflection if property exists
            var paymentsProp = typeof(VenteDetailDto).GetProperty("Payments");
            if (paymentsProp != null)
            {
                paymentsProp.SetValue(vente, payments);
            }

            return vente;
        }

        public async Task<string> GetNextFactureNumberAsync()
        {
            var factureNumbers = await _context.Ventes
                .AsNoTracking()
                .Select(v => v.FactureNumber)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToListAsync();

            var maxNumber = 0;

            foreach (var factureNumber in factureNumbers)
            {
                var value = factureNumber!.Trim();

                if (Regex.IsMatch(value, "^\\d+$") && int.TryParse(value, out var numericValue))
                {
                    maxNumber = Math.Max(maxNumber, numericValue);
                }
            }

            if (maxNumber == 0)
            {
                var maxId = await _context.Ventes.AsNoTracking().MaxAsync(v => (int?)v.Id) ?? 0;
                maxNumber = maxId;
            }

            return (maxNumber + 1).ToString();
        }
    }
}
