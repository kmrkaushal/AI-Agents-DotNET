using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AdventureWorksApi
{
    using Microsoft.EntityFrameworkCore;

    public class AdventureWorksDbContext : DbContext
    {
        public AdventureWorksDbContext(
            DbContextOptions<AdventureWorksDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .ToTable("Product", "SalesLT");

            modelBuilder.Entity<Customer>()
                .ToTable("Customer", "SalesLT");

            base.OnModelCreating(modelBuilder);
        }
    }
}
