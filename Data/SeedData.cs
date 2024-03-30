using projectfiets.Models;

namespace projectfiets.Data
{
    public static class SeedData
    {
        public static void Initialize(FietsContext context)
        {
            if (!context.Fietsen.Any())
            {
                var admin = new Gebruiker { Email = "administrator", Password = "vives", Confirmed = true, Admin = true };
                context.Gebruikers.Add(admin);

                var fietsen = new List<fiets>
                {
                    new fiets { Naam = "Trek MTB", Price = 599.99m, ImageUrl = "mtb1.jpg" },
                    new fiets { Naam = "Cube MTB", Price = 1099.99m, ImageUrl = "mtb2.jpg" },
                    new fiets { Naam = "Sworks MTB", Price = 11099.99m, ImageUrl = "mtb3.jpg" }
                };

                foreach (var fiets in fietsen)
                {
                    context.Fietsen.Add(fiets);
                }

                context.SaveChanges();
            }

        }
    }
}
