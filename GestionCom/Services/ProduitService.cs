using GestionCom.Data;
using GestionCom.DTOs.Produit;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class ProduitService
    {
        private readonly AppDbContext _context;

        public ProduitService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateProduitAsync(CreateProduitDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Designation))
                throw new InvalidOperationException("La désignation est requise.");

            var produit = new Produit
            {
                Marque = Normalize(dto.Marque),
                Famille = Normalize(dto.Famille),
                Designation = dto.Designation.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Produits.Add(produit);
            await _context.SaveChangesAsync();

            return produit.Id;
        }

        public async Task UpdateProduitAsync(int produitId, CreateProduitDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Designation))
                throw new InvalidOperationException("La désignation est requise.");

            var produit = await _context.Produits.FirstOrDefaultAsync(p => p.Id == produitId);

            if (produit == null)
                throw new InvalidOperationException("Produit introuvable.");

            produit.Marque = Normalize(dto.Marque);
            produit.Famille = Normalize(dto.Famille);
            produit.Designation = dto.Designation.Trim();
            produit.UpdatedAt = DateTime.UtcNow;
            produit.UpdatedBy = userId;

            await _context.SaveChangesAsync();
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
