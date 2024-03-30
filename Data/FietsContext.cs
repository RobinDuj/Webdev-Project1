using Microsoft.EntityFrameworkCore;
using projectfiets.Models;
using static projectfiets.Models.Gebruiker;

namespace projectfiets.Data
{
    public class FietsContext : DbContext
    {
        public FietsContext(DbContextOptions<FietsContext> options) : base(options)
        {
        }

        public DbSet<fiets> Fietsen { get; set; }
        public DbSet<Gebruiker> Gebruikers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=fietsenzaak-part1;Integrated Security=true;");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, FietsContext context)
        {
            context.Database.EnsureCreated();
            SeedData.Initialize(context);
        }
    }
}
