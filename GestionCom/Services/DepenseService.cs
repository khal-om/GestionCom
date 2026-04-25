using GestionCom.Data;
using GestionCom.DTOs.Depense;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class DepenseService
    {
        private readonly AppDbContext _context;

        public DepenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateDepenseAsync(CreateDepenseDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Libelle))
                throw new Exception("Le libellé est requis.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new Exception("La description est requise.");

            if (dto.Montant <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            var depense = new Depense
            {
                DateDepense = NormalizeToUtc(dto.DateDepense),
                Libelle = dto.Libelle.Trim(),
                Montant = dto.Montant,
                Description = dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Depenses.Add(depense);
            await _context.SaveChangesAsync();

            return depense.Id;
        }

        public async Task<DepenseOverviewDto> GetOverviewAsync()
        {
            var depenseEntities = await _context.Depenses
                .AsNoTracking()
                .OrderByDescending(d => d.DateDepense)
                .ThenByDescending(d => d.Id)
                .ToListAsync();

            var creatorIds = depenseEntities
                .Where(d => d.CreatedBy.HasValue)
                .Select(d => d.CreatedBy!.Value)
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .AsNoTracking()
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var items = depenseEntities
                .Select(d => new DepenseListItemDto
                {
                    Id = d.Id,
                    DateDepense = d.DateDepense,
                    Libelle = d.Libelle,
                    Montant = d.Montant,
                    Description = d.Description,
                    CreatedByName = d.CreatedBy.HasValue && userNames.TryGetValue(d.CreatedBy.Value, out var username)
                        ? username
                        : "Système"
                })
                .ToList();

            return new DepenseOverviewDto
            {
                Items = items,
                TotalAmount = items.Sum(i => i.Montant)
            };
        }

        public async Task UpdateDepenseAsync(int depenseId, CreateDepenseDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Libelle))
                throw new Exception("Le libellé est requis.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new Exception("La description est requise.");

            if (dto.Montant <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            var depense = await _context.Depenses.FirstOrDefaultAsync(d => d.Id == depenseId);

            if (depense == null)
                throw new Exception("Dépense introuvable.");

            depense.DateDepense = NormalizeToUtc(dto.DateDepense);
            depense.Libelle = dto.Libelle.Trim();
            depense.Montant = dto.Montant;
            depense.Description = dto.Description.Trim();
            depense.UpdatedAt = DateTime.UtcNow;
            depense.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}