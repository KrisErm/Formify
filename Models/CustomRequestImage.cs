using System;

namespace Formify.Models
{
    public class CustomRequestImage
    {
        public long Id { get; set; }
        public long RequestId { get; set; }

        public byte[] ImageData { get; set; } = null!;
        public string? ImageName { get; set; }
        public string? ImageContentType { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreateDate { get; set; }

        public CustomRequest? Request { get; set; }
    }
}
