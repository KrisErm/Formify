namespace Formify.Models
{
    public class DeliveryMethod
    {
        public int Id { get; set; }              // SERIAL
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }   // NUMERIC(10,2)

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
