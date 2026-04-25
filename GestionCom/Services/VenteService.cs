using GestionCom.Data;
using GestionCom.DTOs.Vente;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class VenteService
    {
            private readonly AppDbContext _context;
            private readonly IStockService _stockService;

            public VenteService(AppDbContext context, IStockService stockService)
            {
                _context = context;
                _stockService = stockService;
            }
        
        public async Task<VenteResultDto> CreateVenteAsync(CreateVenteDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            if (dto.ClientId <= 0)
                throw new Exception("Le client est requis.");

            if (dto.Lignes == null || !dto.Lignes.Any())
                throw new Exception("Une vente doit contenir au moins un produit.");

            if (dto.TotalAmount <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);

            if (!clientExists)
                throw new Exception("Client introuvable.");

            foreach (var ligne in dto.Lignes)
            {
                if (ligne.ProduitId <= 0)
                    throw new Exception("Produit invalide.");

                if (ligne.Quantite <= 0)
                    throw new Exception("La quantité doit être supérieure à 0.");

                var produitExists = await _context.Produits.AnyAsync(p => p.Id == ligne.ProduitId);

                if (!produitExists)
                    throw new Exception($"Produit introuvable: {ligne.ProduitId}.");
            }

            var isDelivered = string.Equals(dto.Etat, "Livre", StringComparison.OrdinalIgnoreCase);

            var warnings = new List<string>();

            // Check stock (NON blocking)
            foreach (var ligne in dto.Lignes)
            {
                var stock = await _stockService.GetStockAsync(ligne.ProduitId);

                if (stock < ligne.Quantite)
                {
                    warnings.Add($"Stock insuffisant pour produit {ligne.ProduitId} (manque {ligne.Quantite - stock})");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dateVente = NormalizeToUtc(dto.DateVente);
                var dateLivraison = dto.DateLivraison.HasValue
                    ? NormalizeToUtc(dto.DateLivraison.Value)
                    : (DateTime?)null;

                var vente = new Vente
                {
                    ClientId = dto.ClientId,
                    TotalAmount = dto.TotalAmount,
                    DateVente = dateVente,
                    DateLivraison = isDelivered ? (dateLivraison ?? dateVente) : dateLivraison,
                    Etat = isDelivered ? VenteEtat.Livre : VenteEtat.Commande,
                    Note = dto.Note,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ventes.Add(vente);
                await _context.SaveChangesAsync();

                var lignes = dto.Lignes.Select(l => new VenteLigne
                {
                    VenteId = vente.Id,
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.VenteLignes.AddRange(lignes);

                if (isDelivered)
                {
                    var movements = dto.Lignes.Select(l => new StockMovement
                    {
                        ProduitId = l.ProduitId,
                        Quantity = -l.Quantite,
                        MovementType = "SALE",
                        ReferenceId = vente.Id,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.StockMovements.AddRange(movements);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new VenteResultDto
                {
                    VenteId = vente.Id,
                    Warnings = warnings
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<VenteResultDto> DeliverVenteAsync(int venteId, int userId)
        {
            var vente = await _context.Ventes
                .Include(v => v.Lignes)
                .FirstOrDefaultAsync(v => v.Id == venteId && v.CreatedBy == userId);

            if (vente == null)
                throw new Exception("Vente introuvable.");

            if (vente.Etat == VenteEtat.Livre)
                throw new Exception("La vente est déjà livrée.");

            var warnings = new List<string>();

            // Check stock BEFORE transaction (non-blocking)
            foreach (var ligne in vente.Lignes)
            {
                var stock = await _stockService.GetStockAsync(ligne.ProduitId);

                if (stock < ligne.Quantite)
                {
                    warnings.Add($"Stock insuffisant pour produit {ligne.ProduitId} (manque {ligne.Quantite - stock})");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create stock movements
                var movements = vente.Lignes.Select(l => new StockMovement
                {
                    ProduitId = l.ProduitId,
                    Quantity = -l.Quantite,
                    MovementType = "SALE",
                    ReferenceId = vente.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.StockMovements.AddRange(movements);

                // Update state
                vente.Etat = VenteEtat.Livre;
                vente.DateLivraison = DateTime.UtcNow;
                vente.UpdatedBy = userId;
                vente.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new VenteResultDto
                {
                    VenteId = vente.Id,
                    Warnings = warnings
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<VenteResultDto> UpdateVenteAsync(int venteId, UpdateVenteDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            var vente = await _context.Ventes
                .Include(v => v.Lignes)
                .FirstOrDefaultAsync(v => v.Id == venteId && v.CreatedBy == userId);

            if (vente == null)
                throw new Exception("Vente introuvable.");

            if (vente.Etat == VenteEtat.Livre)
                throw new Exception("Impossible de modifier une vente livrée.");

            if (dto.Lignes == null || !dto.Lignes.Any())
                throw new Exception("Une vente doit contenir au moins un produit.");

            if (dto.TotalAmount <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);

            if (!clientExists)
                throw new Exception("Client introuvable.");

            var warnings = new List<string>();

            // Stock check (non-blocking)
            foreach (var ligne in dto.Lignes)
            {
                if (ligne.ProduitId <= 0)
                    throw new Exception("Produit invalide.");

                if (ligne.Quantite <= 0)
                    throw new Exception("La quantité doit être supérieure à 0.");

                var produitExists = await _context.Produits.AnyAsync(p => p.Id == ligne.ProduitId);

                if (!produitExists)
                    throw new Exception($"Produit introuvable: {ligne.ProduitId}.");

                var stock = await _stockService.GetStockAsync(ligne.ProduitId);

                if (stock < ligne.Quantite)
                {
                    warnings.Add($"Stock insuffisant pour produit {ligne.ProduitId} (manque {ligne.Quantite - stock})");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dateVente = NormalizeToUtc(dto.DateVente);
                var dateLivraison = dto.DateLivraison.HasValue
                    ? NormalizeToUtc(dto.DateLivraison.Value)
                    : (DateTime?)null;

                // Update main fields
                vente.ClientId = dto.ClientId;
                vente.TotalAmount = dto.TotalAmount;
                vente.DateVente = dateVente;
                vente.DateLivraison = dateLivraison;
                vente.Note = dto.Note;

                vente.UpdatedBy = userId;
                vente.UpdatedAt = DateTime.UtcNow;

                // Remove existing lignes
                _context.VenteLignes.RemoveRange(vente.Lignes);

                // Add new lignes
                var newLignes = dto.Lignes.Select(l => new VenteLigne
                {
                    VenteId = vente.Id,
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.VenteLignes.AddRange(newLignes);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new VenteResultDto
                {
                    VenteId = vente.Id,
                    Warnings = warnings
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

    }
}
