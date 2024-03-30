namespace projectfiets.Models
{
    public class fiets
    {
        public int ID { get; set; }
        public string Naam { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
