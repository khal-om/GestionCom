using GestionCom.Data;
using GestionCom.DTOs.Profile;
using GestionCom.Entities;
using GestionCom.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class UserManagementService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(AppDbContext context, PasswordService passwordService, ILogger<UserManagementService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role.Name
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException("Utilisateur introuvable.");

            return user;
        }

        public async Task UpdateProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            if (dto == null)
                throw new InvalidOperationException("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new InvalidOperationException("Le nom d'utilisateur est requis.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur introuvable.");

            var usernameExists = await _context.Users.AnyAsync(u => u.Id != userId && u.Username == dto.Username.Trim());
            if (usernameExists)
                throw new InvalidOperationException("Ce nom d'utilisateur existe déjà.");

            user.Username = dto.Username.Trim();
            user.Email = Normalize(dto.Email);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto == null)
                throw new InvalidOperationException("Données invalides.");

            if (!string.Equals(dto.NewPassword, dto.ConfirmPassword, StringComparison.Ordinal))
                throw new InvalidOperationException("La confirmation du mot de passe ne correspond pas.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur introuvable.");

            if (!_passwordService.VerifyPassword(user, dto.CurrentPassword))
                throw new InvalidOperationException("Le mot de passe actuel est incorrect.");

            user.PasswordHash = _passwordService.HashPassword(user, dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task<List<RoleOptionDto>> GetRolesAsync()
        {
            // Include roles even if global soft-delete query filters are in place
            var roles = await _context.Roles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RoleOptionDto
                {
                    RoleId = r.Id,
                    Name = r.Name
                })
                .ToListAsync();

            _logger.LogInformation("GetRolesAsync returned {Count} roles", roles.Count);

            return roles;
        }

        public async Task<int> CreateUserAsync(CreateUserDto dto, int adminUserId)
        {
            if (dto == null)
                throw new InvalidOperationException("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new InvalidOperationException("Le nom d'utilisateur est requis.");

            if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
                throw new InvalidOperationException("La confirmation du mot de passe ne correspond pas.");

            var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username.Trim());
            if (usernameExists)
                throw new InvalidOperationException("Ce nom d'utilisateur existe déjà.");

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == dto.RoleId);
            if (!roleExists)
                throw new InvalidOperationException("Rôle introuvable.");

            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = Normalize(dto.Email),
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUserId
            };

            user.PasswordHash = _passwordService.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
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
