using GestionCom.Data;
using GestionCom.DTOs.Achat;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class AchatService
    {
        private readonly AppDbContext _context;

        public AchatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AchatResultDto> CreateAchatAsync(CreateAchatDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            await ValidateAchatAsync(dto.Fournisseur, dto.TotalAmount, dto.PaidAmount, dto.Lignes);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dateAchat = NormalizeToUtc(dto.DateAchat);

                var achat = new Achat
                {
                    Fournisseur = dto.Fournisseur.Trim(),
                    TotalAmount = dto.TotalAmount,
                    PaidAmount = dto.PaidAmount,
                    DateAchat = dateAchat,
                    Note = Normalize(dto.Note),
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Achats.Add(achat);
                await _context.SaveChangesAsync();

                var lignes = dto.Lignes.Select(l => new AchatLigne
                {
                    AchatId = achat.Id,
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.AchatLignes.AddRange(lignes);

                var movements = dto.Lignes.Select(l => new StockMovement
                {
                    ProduitId = l.ProduitId,
                    Quantity = l.Quantite,
                    MovementType = "PURCHASE",
                    ReferenceId = achat.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.StockMovements.AddRange(movements);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new AchatResultDto
                {
                    AchatId = achat.Id,
                    RemainingAmount = achat.TotalAmount - achat.PaidAmount
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AchatResultDto> UpdateAchatAsync(int achatId, UpdateAchatDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            await ValidateAchatAsync(dto.Fournisseur, dto.TotalAmount, dto.PaidAmount, dto.Lignes);

            var achat = await _context.Achats
                .Include(a => a.Lignes)
                .FirstOrDefaultAsync(a => a.Id == achatId && a.CreatedBy == userId);

            if (achat == null)
                throw new Exception("Achat introuvable.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dateAchat = NormalizeToUtc(dto.DateAchat);

                achat.Fournisseur = dto.Fournisseur.Trim();
                achat.TotalAmount = dto.TotalAmount;
                achat.PaidAmount = dto.PaidAmount;
                achat.DateAchat = dateAchat;
                achat.Note = Normalize(dto.Note);
                achat.UpdatedBy = userId;
                achat.UpdatedAt = DateTime.UtcNow;

                var existingMovements = await _context.StockMovements
                    .Where(m => m.ReferenceId == achatId && m.MovementType == "PURCHASE")
                    .ToListAsync();

                _context.AchatLignes.RemoveRange(achat.Lignes);
                _context.StockMovements.RemoveRange(existingMovements);

                var lignes = dto.Lignes.Select(l => new AchatLigne
                {
                    AchatId = achat.Id,
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                var movements = dto.Lignes.Select(l => new StockMovement
                {
                    ProduitId = l.ProduitId,
                    Quantity = l.Quantite,
                    MovementType = "PURCHASE",
                    ReferenceId = achat.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.AchatLignes.AddRange(lignes);
                _context.StockMovements.AddRange(movements);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new AchatResultDto
                {
                    AchatId = achat.Id,
                    RemainingAmount = achat.TotalAmount - achat.PaidAmount
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ValidateAchatAsync(string? fournisseur, decimal totalAmount, decimal paidAmount, IEnumerable<CreateAchatLigneDto>? lignes)
        {
            if (string.IsNullOrWhiteSpace(fournisseur))
                throw new Exception("Le fournisseur est requis.");

            if (lignes == null || !lignes.Any())
                throw new Exception("Un achat doit contenir au moins un produit.");

            if (totalAmount <= 0)
                throw new Exception("Le montant total doit être supérieur à 0.");

            if (paidAmount < 0)
                throw new Exception("Le montant payé ne peut pas être négatif.");

            if (paidAmount > totalAmount)
                throw new Exception("Le montant payé ne peut pas dépasser le montant total.");

            foreach (var ligne in lignes)
            {
                if (ligne.ProduitId <= 0)
                    throw new Exception("Produit invalide.");

                if (ligne.Quantite <= 0)
                    throw new Exception("La quantité doit être supérieure à 0.");

                var produitExists = await _context.Produits.AnyAsync(p => p.Id == ligne.ProduitId);

                if (!produitExists)
                    throw new Exception($"Produit introuvable: {ligne.ProduitId}.");
            }
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
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
