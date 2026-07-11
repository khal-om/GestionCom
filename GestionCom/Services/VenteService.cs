using System.Globalization;
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

                if (ligne.PrixUnitaire.HasValue && ligne.PrixUnitaire.Value < 0)
                    throw new Exception("Le prix unitaire doit être supérieur ou égal à 0.");

                if (ligne.RemisePercent < 0 || ligne.RemisePercent > 100)
                    throw new Exception("La remise doit être comprise entre 0 et 100 %.");

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
                    DocumentNumber = await GenerateDocumentNumberAsync(),
                    TotalAmount = dto.TotalAmount,
                    DateVente = dateVente,
                    DateLivraison = isDelivered ? (dateLivraison ?? dateVente) : dateLivraison,
                    Etat = isDelivered ? VenteEtat.Livre : VenteEtat.Commande,
                    FactureNumber = null,
                    Note = dto.Note,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ventes.Add(vente);
                await _context.SaveChangesAsync();

                vente.FactureNumber = NormalizeFactureNumber(dto.FactureNumber) ?? vente.Id.ToString(CultureInfo.InvariantCulture);
                await _context.SaveChangesAsync();

                var lignes = dto.Lignes.Select(l => new VenteLigne
                {
                    VenteId = vente.Id,
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    PrixUnitaire = NormalizePrice(l.PrixUnitaire),
                    RemisePercent = NormalizePercent(l.RemisePercent),
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

        public async Task<VenteResultDto> DeliverVenteAsync(int venteId, int userId, bool isAdmin = false)
        {
            var vente = await _context.Ventes
                .Include(v => v.Lignes)
                .FirstOrDefaultAsync(v => v.Id == venteId && (isAdmin || v.CreatedBy == userId));

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

        public async Task<VenteResultDto> UpdateVenteAsync(int venteId, UpdateVenteDto dto, int userId, bool isAdmin = false)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            var vente = await _context.Ventes
                .Include(v => v.Lignes)
                .FirstOrDefaultAsync(v => v.Id == venteId && (isAdmin || v.CreatedBy == userId));

            if (vente == null)
                throw new Exception("Vente introuvable.");

            // Allow updating a vente regardless of its state (Commande or Livrée).
            // Previous behavior prevented modifying a delivered sale; change requested
            // allows edits even after delivery.

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

                if (ligne.PrixUnitaire.HasValue && ligne.PrixUnitaire.Value < 0)
                    throw new Exception("Le prix unitaire doit être supérieur ou égal à 0.");

                if (ligne.RemisePercent < 0 || ligne.RemisePercent > 100)
                    throw new Exception("La remise doit être comprise entre 0 et 100 %.");

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
                vente.FactureNumber = NormalizeFactureNumber(dto.FactureNumber) ?? vente.Id.ToString(CultureInfo.InvariantCulture);
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
                    PrixUnitaire = NormalizePrice(l.PrixUnitaire),
                    RemisePercent = NormalizePercent(l.RemisePercent),
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.VenteLignes.AddRange(newLignes);

                // If the vente is already delivered, update stock movements to reflect the new lignes.
                if (vente.Etat == VenteEtat.Livre)
                {
                    // Remove previous SALE movements for this vente
                    var existingMovements = await _context.StockMovements
                        .Where(m => m.ReferenceId == vente.Id && m.MovementType == "SALE")
                        .ToListAsync();

                    if (existingMovements.Any())
                    {
                        _context.StockMovements.RemoveRange(existingMovements);
                    }

                    // Add new SALE movements according to updated lignes
                    var newMovements = newLignes.Select(l => new StockMovement
                    {
                        ProduitId = l.ProduitId,
                        Quantity = -l.Quantite,
                        MovementType = "SALE",
                        ReferenceId = vente.Id,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.StockMovements.AddRange(newMovements);
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

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static decimal? NormalizePrice(decimal? value)
            => value.HasValue
                ? decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero)
                : null;

        private static decimal NormalizePercent(decimal value)
            => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

        private static string? NormalizeFactureNumber(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private async Task<string> GenerateDocumentNumberAsync()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            while (true)
            {
                var now = DateTime.UtcNow;
                var suffix = string.Create(5, alphabet, static (buffer, chars) =>
                {
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = chars[Random.Shared.Next(chars.Length)];
                    }
                });

                var candidate = $"V-{now:dd-MM-yy}-{suffix}";

                if (!await _context.Ventes.AnyAsync(v => v.DocumentNumber == candidate))
                {
                    return candidate;
                }
            }
        }

    }
}
