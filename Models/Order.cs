using System;

namespace Formify.Models
{
    public class Order
    {
        public long Id { get; set; }                  // BIGSERIAL
        public long UserId { get; set; }
        public int StatusId { get; set; }
        public int DeliveryMethodId { get; set; }
        public long? CustomRequestId { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? DeliveryPrice { get; set; }

        public string? Comment { get; set; }
        public string? DeliveryFullName { get; set; }
        public string? DeliveryPhone { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryPostalCode { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public User? User { get; set; }
        public OrderStatus? Status { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }
        public CustomRequest? CustomRequest { get; set; }
    }
}
