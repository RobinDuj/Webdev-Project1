namespace projectfiets.Models
{
    public class Gebruiker
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Confirmed { get; set; }
        public bool Admin { get; set; }
        
        public ICollection<Order> Orders { get; set; }
    }
}
