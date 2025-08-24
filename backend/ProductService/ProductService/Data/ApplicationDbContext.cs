using Microsoft.EntityFrameworkCore;
using ProductService.Models;


namespace ProductService.Data
{
    public class ApplicationDbContext (DbContextOptions<ApplicationDbContext> options): DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products","inventory");
                e.HasKey(p => p.Id);
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.Description).HasMaxLength(1000);
                e.Property(p => p.Category).IsRequired().HasMaxLength(100);
                e.Property(p => p.ImageUrl).HasMaxLength(500);
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.Stock);
                e.Property(p => p.CreatedAt).HasPrecision(3);
                e.Property(p => p.UpdatedAt).HasPrecision(3);
                e.HasIndex(p => p.Name).IsUnique();
            });
        }
    }     
}
