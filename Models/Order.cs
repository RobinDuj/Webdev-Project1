namespace projectfiets.Models
{
    public class Order
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public DateTime? CreateOrder { get; set; }
        public decimal? price { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
