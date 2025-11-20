using Microsoft.EntityFrameworkCore;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        // Aici definim tabelul de produse
        public DbSet<Product> Products { get; set; }
    }
}