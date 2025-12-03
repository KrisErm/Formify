using System;

namespace Formify.Models
{
    public class CustomRequest
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int StatusId { get; set; }

        public string? CommentUser { get; set; }
        public string? CommentAdmin { get; set; }
        public decimal? FinalPrice { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public User? User { get; set; }
        public RequestStatus? Status { get; set; }

        public ICollection<CustomRequestItem> Items { get; set; } = new List<CustomRequestItem>();
        public ICollection<CustomRequestImage> Images { get; set; } = new List<CustomRequestImage>();
    }
}
