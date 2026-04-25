using GestionCom.Data;
using GestionCom.DTOs.Achat;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class AchatQueryService
    {
        private readonly AppDbContext _context;

        public AchatQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AchatListItemDto>> GetAchatsAsync(int userId)
        {
            return await _context.Achats
                .AsNoTracking()
                .Where(a => a.CreatedBy == userId)
                .OrderByDescending(a => a.DateAchat)
                .ThenByDescending(a => a.Id)
                .Select(a => new AchatListItemDto
                {
                    Id = a.Id,
                    Fournisseur = a.Fournisseur,
                    DateAchat = a.DateAchat,
                    TotalAmount = a.TotalAmount,
                    PaidAmount = a.PaidAmount,
                    RemainingAmount = a.TotalAmount - a.PaidAmount,
                    LineCount = a.Lignes.Count
                })
                .ToListAsync();
        }

        public async Task<AchatDetailDto> GetAchatDetailAsync(int achatId, int userId)
        {
            var achat = await _context.Achats
                .AsNoTracking()
                .Where(a => a.Id == achatId && a.CreatedBy == userId)
                .Select(a => new AchatDetailDto
                {
                    Id = a.Id,
                    Fournisseur = a.Fournisseur,
                    DateAchat = a.DateAchat,
                    TotalAmount = a.TotalAmount,
                    PaidAmount = a.PaidAmount,
                    RemainingAmount = a.TotalAmount - a.PaidAmount,
                    Note = a.Note,
                    Lignes = a.Lignes
                        .OrderBy(l => l.Id)
                        .Select(l => new AchatDetailLineDto
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

            if (achat == null)
                throw new InvalidOperationException("Achat introuvable.");

            return achat;
        }
    }
}
