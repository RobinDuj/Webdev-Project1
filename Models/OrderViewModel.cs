namespace projectfiets.Models
{
    public class OrderViewModel
    {
        public IEnumerable<fiets> Fietsen { get; set; }
        public Dictionary<int, int> FietsQuantities { get; set; }
        public string Admin { get; set; }
    }
}
