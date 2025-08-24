
using Microsoft.EntityFrameworkCore;
using TransactionsService.Entities;

namespace TransactionsService.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<InventoryTransaction> Transactions => Set<InventoryTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryTransaction>(e =>
            {
                e.ToTable("InventoryTransactions", "inventory");
                e.HasKey(t => t.Id);

                e.Property(t => t.Date).HasPrecision(3);
                e.Property(t => t.Type).HasConversion<byte>();
                e.Property(t => t.UnitPrice).HasPrecision(18, 2);
                e.Property(t => t.Total).HasPrecision(18, 2)
                    .HasComputedColumnSql("[Quantity]*[UnitPrice]", stored: true);
                e.Property(t => t.Detail).HasMaxLength(1000);

                e.HasIndex(t => new { t.ProductId, t.Date });
                e.HasIndex(t => new { t.Type, t.Date });
            });
        }
    }
}
