using GestionCom.Data;
using GestionCom.DTOs.Vente;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class VenteQueryService
    {
        private readonly AppDbContext _context;

        public VenteQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenteListItemDto>> GetVentesAsync(int userId)
        {
            return await _context.Ventes
                .AsNoTracking()
                .Where(v => v.CreatedBy == userId)
                .OrderByDescending(v => v.DateVente)
                .ThenByDescending(v => v.Id)
                .Select(v => new VenteListItemDto
                {
                    Id = v.Id,
                    ClientId = v.ClientId,
                    ClientName = v.Client.Name,
                    DateVente = v.DateVente,
                    TotalAmount = v.TotalAmount,
                    Etat = v.Etat == VenteEtat.Livre ? "Livrée" : "Commande",
                    LineCount = v.Lignes.Count
                })
                .ToListAsync();
        }

        public async Task<VenteDetailDto> GetVenteDetailAsync(int venteId, int userId)
        {
            var vente = await _context.Ventes
                .AsNoTracking()
                .Where(v => v.Id == venteId && v.CreatedBy == userId)
                .Select(v => new VenteDetailDto
                {
                    Id = v.Id,
                    ClientId = v.ClientId,
                    ClientName = v.Client.Name,
                    DateVente = v.DateVente,
                    DateLivraison = v.DateLivraison,
                    TotalAmount = v.TotalAmount,
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
                            CurrentStock = l.Produit.StockMovements.Sum(m => (int?)m.Quantity) ?? 0
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (vente == null)
                throw new InvalidOperationException("Vente introuvable.");

            return vente;
        }
    }
}
