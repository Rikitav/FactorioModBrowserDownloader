using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FactorioNexus.ApplicationArchitecture.DataBases
{
    public class IndexedModPortalDatabase : DbContext
    {
        private static readonly string DatabaseFileName = Path.Combine(App.DataDirectory, "mods.db");
        private static readonly string DatabaseConnection = string.Format("Data Source={0};Cache=Shared", DatabaseFileName);

        public DbSet<ModEntryEntity> Items { get; set; }

        public IndexedModPortalDatabase() : base()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(DatabaseConnection);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModEntryEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OwnerName).HasMaxLength(100);
                entity.Property(e => e.Summary).HasMaxLength(1000);
                entity.Property(e => e.DownloadsCount);
                entity.Property(e => e.Score);
                entity.Property(e => e.Releases).HasConversion(new JsonValueConverter<ReleaseInfo[]>());
                entity.Property(e => e.Category).HasConversion(new CategoryConverter());

                /*
                entity.OwnsOne(e => e.Category, category =>
                {
                    category.Property(c => c.Name).HasMaxLength(50);
                    category.Property(c => c.Title).HasMaxLength(100);
                    category.Property(c => c.Description).HasMaxLength(500);
                });
                */

                entity.OwnsOne(e => e.LatestRelease, release =>
                {
                    release.Property(r => r.DownloadUrl).HasMaxLength(500);
                    release.Property(r => r.FileName).HasMaxLength(200);
                    release.Property(r => r.ReleasedDate);
                    release.Property(r => r.Version).HasConversion(new VersionConverter());
                    release.Property(r => r.ShaHash).HasMaxLength(40);

                    release.OwnsOne(r => r.ModInfo, modInfo =>
                    {
                        modInfo.Property(m => m.FactorioVersion).HasConversion(new VersionConverter());
                        modInfo.Property(m => m.Dependencies).HasConversion(new JsonValueConverter<DependencyInfo[]>());
                    });
                });
            });
        }
    }
}
