using System;
using System.Collections.Generic;

namespace Formify.Models
{
    public class Cart
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public User? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
