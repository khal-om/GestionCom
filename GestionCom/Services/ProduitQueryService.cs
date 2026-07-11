using GestionCom.Data;
using GestionCom.DTOs.Produit;
using GestionCom.DTOs.Vente;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class ProduitQueryService
    {
        private readonly AppDbContext _context;

        public ProduitQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenteProductOptionDto>> GetProductOptionsAsync()
        {
            return await _context.Produits
                .AsNoTracking()
                .OrderBy(p => p.Marque)
                .ThenBy(p => p.Designation)
                .Select(p => new VenteProductOptionDto
                {
                    Id = p.Id,
                    Label = string.IsNullOrWhiteSpace(p.Marque)
                        ? p.Designation
                        : p.Marque + " - " + p.Designation,
                    Stock = p.StockMovements.Sum(m => (int?)m.Quantity) ?? 0,
                    PrixUnitaire = p.PrixUnitaire
                })
                .ToListAsync();
        }

        public async Task<List<ProduitListItemDto>> GetProductsAsync()
        {
            return await _context.Produits
                .AsNoTracking()
                .OrderBy(p => p.Marque)
                .ThenBy(p => p.Designation)
                .Select(p => new ProduitListItemDto
                {
                    Id = p.Id,
                    Marque = p.Marque,
                    Famille = p.Famille,
                    Designation = p.Designation,
                    PrixUnitaire = p.PrixUnitaire,
                    Stock = p.StockMovements.Sum(m => (int?)m.Quantity) ?? 0
                })
                .ToListAsync();
        }
    }
}
