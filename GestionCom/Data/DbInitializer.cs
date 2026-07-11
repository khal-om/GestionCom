using GestionCom.Entities;
using GestionCom.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Data
{
    public class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            EnsureVenteDocumentNumberSchema(context);
            EnsureRoles(context);
            EnsureAdminUser(context);
        }

        private static void EnsureVenteDocumentNumberSchema(AppDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                ALTER TABLE ""Ventes""
                ADD COLUMN IF NOT EXISTS ""DocumentNumber"" text NOT NULL DEFAULT '';
            ");

            context.Database.ExecuteSqlRaw(@"
                UPDATE ""Ventes""
                SET ""DocumentNumber"" = 'V-' || TO_CHAR(COALESCE(""DateVente"", NOW()), 'DD-MM-YY') || '-' || UPPER(SUBSTRING(MD5(""Id""::text || '-' || COALESCE(""CreatedAt""::text, NOW()::text)) FROM 1 FOR 5))
                WHERE COALESCE(""DocumentNumber"", '') = ''
                   OR ""DocumentNumber"" NOT LIKE 'V-__-__-__-_____'
                   OR SUBSTRING(""DocumentNumber"" FROM 3 FOR 8) <> TO_CHAR(COALESCE(""DateVente"", NOW()), 'DD-MM-YY');
            ");

            context.Database.ExecuteSqlRaw(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Ventes_DocumentNumber""
                ON ""Ventes"" (""DocumentNumber"");
            ");
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
