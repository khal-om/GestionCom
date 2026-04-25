using GestionCom.Entities;
using GestionCom.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Data
{
    public class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            EnsureRoles(context);
            EnsureAdminUser(context);
        }

        private static void EnsureRoles(AppDbContext context)
        {
            var requiredRoles = new[] { "Admin", "User" };
            var existingRoles = context.Roles
                .IgnoreQueryFilters()
                .Select(r => r.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingRoles = requiredRoles
                .Where(roleName => !existingRoles.Contains(roleName))
                .Select(roleName => new Role
                {
                    Name = roleName,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (missingRoles.Count > 0)
            {
                context.Roles.AddRange(missingRoles);
                context.SaveChanges();
            }
        }

        private static void EnsureAdminUser(AppDbContext context)
        {
            var hasUsers = context.Users.IgnoreQueryFilters().Any();
            if (hasUsers)
            {
                return;
            }

            var adminRole = context.Roles
                .IgnoreQueryFilters()
                .First(r => r.Name == "Admin");

            var passwordService = new PasswordService();

            var user = new User
            {
                Username = "admin",
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = passwordService.HashPassword(user, "admin123");

            context.Users.Add(user);
            context.SaveChanges();
        }
    }
}
