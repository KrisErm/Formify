namespace Formify.Models
{
    public class RequestStatus
    {
        public int Id { get; set; }              // SERIAL
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public ICollection<CustomRequest> Requests { get; set; } = new List<CustomRequest>();
    }
}
