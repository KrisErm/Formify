namespace Formify.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }              // SERIAL
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
