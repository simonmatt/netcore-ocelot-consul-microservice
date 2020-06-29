using Microsoft.EntityFrameworkCore;

namespace Product.API.Models
{
    public class ProductDbContext:DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options):base(options){}

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().HasData(new Product[]
            {
                new Product
                {
                    ID = 1,
                    Name = "Product #1",
                    Stock = 100
                },
                new Product
                {
                    ID = 2,
                    Name = "Product #2",
                    Stock = 100
                }
            });
        }
    }
}