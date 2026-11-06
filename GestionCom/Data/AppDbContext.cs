using Microsoft.EntityFrameworkCore;
using GestionCom.Entities;
using GestionCom.Features.Showroom.Entities;
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

        // Independent public showroom catalogue and editable content.
        public DbSet<ShowroomBrand> ShowroomBrands => Set<ShowroomBrand>();
        public DbSet<ShowroomRange> ShowroomRanges => Set<ShowroomRange>();
        public DbSet<ShowroomProduct> ShowroomProducts => Set<ShowroomProduct>();
        public DbSet<ShowroomProductImage> ShowroomProductImages => Set<ShowroomProductImage>();
        public DbSet<ShowroomPageSection> ShowroomPageSections => Set<ShowroomPageSection>();
        public DbSet<ShowroomService> ShowroomServices => Set<ShowroomService>();
        public DbSet<ShowroomArticle> ShowroomArticles => Set<ShowroomArticle>();
        public DbSet<ShowroomRoutine> ShowroomRoutines => Set<ShowroomRoutine>();
        public DbSet<ShowroomRoutineProduct> ShowroomRoutineProducts => Set<ShowroomRoutineProduct>();
        public DbSet<ShowroomSiteSetting> ShowroomSiteSettings => Set<ShowroomSiteSetting>();
        public DbSet<ShowroomNavigationItem> ShowroomNavigationItems => Set<ShowroomNavigationItem>();


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

            // Showroom schema is intentionally separate from Produits, stock, sales and purchases.
            modelBuilder.Entity<ShowroomBrand>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
                entity.Property(x => x.NameFr).HasMaxLength(200).IsRequired();
                entity.Property(x => x.AccentColor).HasMaxLength(32);
            });

            modelBuilder.Entity<ShowroomRange>(entity =>
            {
                entity.HasIndex(x => new { x.BrandId, x.Slug }).IsUnique();
                entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
                entity.Property(x => x.NameFr).HasMaxLength(200).IsRequired();
                entity.HasOne(x => x.Brand).WithMany(x => x.Ranges)
                    .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ShowroomProduct>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.HasIndex(x => new { x.IsPublished, x.SortOrder });
                entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
                entity.Property(x => x.NameFr).HasMaxLength(240).IsRequired();
                entity.Property(x => x.PriceTnd).HasPrecision(18, 2);
                entity.HasOne(x => x.Brand).WithMany(x => x.Products)
                    .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Range).WithMany(x => x.Products)
                    .HasForeignKey(x => x.RangeId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ShowroomProductImage>(entity =>
            {
                entity.Property(x => x.ImagePath).HasMaxLength(500).IsRequired();
                entity.HasIndex(x => new { x.ProductId, x.SortOrder });
                entity.HasOne(x => x.Product).WithMany(x => x.Images)
                    .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => !x.Product.IsDeleted);
            });

            modelBuilder.Entity<ShowroomPageSection>(entity =>
            {
                entity.HasIndex(x => new { x.PageKey, x.SectionKey }).IsUnique();
                entity.Property(x => x.PageKey).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SectionKey).HasMaxLength(120).IsRequired();
                entity.Property(x => x.SectionType).HasMaxLength(80).IsRequired();
                entity.Property(x => x.StyleVariant).HasMaxLength(80);
            });

            modelBuilder.Entity<ShowroomService>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
                entity.Property(x => x.TitleFr).HasMaxLength(240).IsRequired();
            });

            modelBuilder.Entity<ShowroomArticle>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
                entity.Property(x => x.TitleFr).HasMaxLength(300).IsRequired();
            });

            modelBuilder.Entity<ShowroomRoutine>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
                entity.Property(x => x.TitleFr).HasMaxLength(240).IsRequired();

            });

            modelBuilder.Entity<ShowroomRoutineProduct>(entity =>
            {
                entity.HasIndex(x => new { x.RoutineId, x.ProductId }).IsUnique();
                entity.HasOne(x => x.Routine).WithMany(x => x.Products)
                    .HasForeignKey(x => x.RoutineId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Product).WithMany(x => x.RoutineLinks)
                    .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => !x.Product.IsDeleted && !x.Routine.IsDeleted);
            });

            modelBuilder.Entity<ShowroomSiteSetting>(entity =>
            {
                entity.HasIndex(x => x.Key).IsUnique();
                entity.Property(x => x.Key).HasMaxLength(160).IsRequired();
            });

            modelBuilder.Entity<ShowroomNavigationItem>(entity =>
            {
                entity.HasIndex(x => x.Key).IsUnique();
                entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
                entity.Property(x => x.LabelFr).HasMaxLength(160).IsRequired();
                entity.Property(x => x.Url).HasMaxLength(500).IsRequired();
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
