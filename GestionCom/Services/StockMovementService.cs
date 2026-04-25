using GestionCom.Data;
using GestionCom.DTOs.Stock;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class StockMovementService
    {
        private readonly AppDbContext _context;

        public StockMovementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddManualEntryAsync(CreateStockMovementDto dto, int userId)
        {
            if (dto.ProduitId <= 0)
                throw new InvalidOperationException("Le produit est requis.");

            if (dto.Quantity <= 0)
                throw new InvalidOperationException("La quantité doit être supérieure à 0.");

            var produitExists = await _context.Produits.AnyAsync(p => p.Id == dto.ProduitId);

            if (!produitExists)
                throw new InvalidOperationException("Produit introuvable.");

            var movement = new StockMovement
            {
                ProduitId = dto.ProduitId,
                Quantity = dto.Quantity,
                MovementType = "MANUAL_ENTRY",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
        }
    }
}
