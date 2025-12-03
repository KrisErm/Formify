namespace Formify.Models
{
    public class CustomRequestItem
    {
        public long Id { get; set; }
        public long RequestId { get; set; }

        public string TypeName { get; set; } = null!;  // тип формочки
        public int Quantity { get; set; }
        public string? Note { get; set; }

        public CustomRequest? Request { get; set; }
    }
}
