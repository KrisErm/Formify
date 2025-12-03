using System;

namespace Formify.Models
{
    public class Product
    {
        public long Id { get; set; }                // BIGSERIAL
        public string Name { get; set; } = null!;   // name
        public string? Description { get; set; }    // description
        public decimal Price { get; set; }          // price numeric(10,2)

        public byte[]? ImageData { get; set; }      // image_data
        public string? ImageName { get; set; }      // image_name
        public string? ImageContentType { get; set; } // image_content_type

        public bool IsActive { get; set; } = true;  // is_active
        public DateTime CreateDate { get; set; }    // create_date
        public DateTime UpdateDate { get; set; }    // update_date
    }
}
