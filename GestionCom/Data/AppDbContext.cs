using Microsoft.EntityFrameworkCore;
using GestionCom.Entities;
using System.Reflection.Emit;

namespace GestionCom.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Produit> Produits => Set<Produit>();
        public DbSet<Achat> Achats => Set<Achat>();
        public DbSet<AchatLigne> AchatLignes => Set<AchatLigne>();
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
        public DbSet<Depense> Depenses => Set<Depense>();
        public DbSet<AppSetting> AppSettings => Set<AppSetting>();
        public DbSet<Vente> Ventes => Set<Vente>();
        public DbSet<VenteLigne> VenteLignes => Set<VenteLigne>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        public DbSet<SavingsEntry> SavingsEntries => Set<SavingsEntry>();


        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(e => e.EventDate);

            modelBuilder.Entity<Depense>()
                .HasIndex(d => d.DateDepense);

            modelBuilder.Entity<AppSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.CalendarEventId)
                .IsUnique();

            modelBuilder.Entity<Vente>()
                .HasQueryFilter(v => !v.Client.IsDeleted);

            modelBuilder.Entity<Vente>()
                .HasIndex(v => v.DocumentNumber)
                .IsUnique();

            modelBuilder.Entity<VenteLigne>()
                .HasQueryFilter(l => !l.Produit.IsDeleted);

            modelBuilder.Entity<AchatLigne>()
                .HasQueryFilter(l => !l.Produit.IsDeleted);

            modelBuilder.Entity<StockMovement>()
                .HasQueryFilter(m => !m.Produit.IsDeleted);

            // Apply global soft delete filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }

            // Vente → VenteLigne (1:N)
            modelBuilder.Entity<Achat>()
                .HasMany(a => a.Lignes)
                .WithOne(l => l.Achat)
                .HasForeignKey(l => l.AchatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AchatLigne>()
                .HasOne(l => l.Produit)
                .WithMany(p => p.AchatLignes)
                .HasForeignKey(l => l.ProduitId);

            modelBuilder.Entity<Vente>()
                .HasMany(v => v.Lignes)
                .WithOne(l => l.Vente)
                .HasForeignKey(l => l.VenteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Produit → VenteLigne
            modelBuilder.Entity<VenteLigne>()
                .HasOne(l => l.Produit)
                .WithMany(p => p.VenteLignes)
                .HasForeignKey(l => l.ProduitId);

            modelBuilder.Entity<Produit>()
                .Property(p => p.PrixUnitaire)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<VenteLigne>()
                .Property(l => l.PrixUnitaire)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<VenteLigne>()
                .Property(l => l.RemisePercent)
                .HasColumnType("numeric(5,2)");

            // Client → Vente
            modelBuilder.Entity<Vente>()
                .HasOne(v => v.Client)
                .WithMany(c => c.Ventes)
                .HasForeignKey(v => v.ClientId);

            // Client → Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClientId);

            modelBuilder.Entity<SavingsEntry>(entity =>
            {
                // The actual table created by previous migrations is named "SavingEntries"
                // (note the singular 'Saving' prefix). Map the entity to that table name
                // so EF queries the correct relation at runtime.
                entity.ToTable("SavingEntries");
                entity.HasIndex(e => e.Date).IsUnique();
                // The database column created by the migration is named "Amount".
                // Map the CLR property AmountSaved to that column and set precision
                // to match the migration (numeric(8,2)).
                entity.Property(e => e.AmountSaved)
                    .HasColumnName("Amount")
                    .HasPrecision(8, 2);
            });
        }

        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : SoftDeletableEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => !e.IsDeleted);
        }

    }

}
