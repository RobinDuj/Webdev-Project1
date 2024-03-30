namespace projectfiets.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int FietsID { get; set; }
        public fiets Fiets { get; set; }

        public int Quantity { get; set; }
    }
}
