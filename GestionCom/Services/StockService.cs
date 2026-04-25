using GestionCom.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class StockService : IStockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetStockAsync(int produitId)
        {
            return await _context.StockMovements
                .Where(m => m.ProduitId == produitId)
                .SumAsync(m => (int?)m.Quantity) ?? 0;
        }
    }
}
