using System.Collections.Generic;

namespace Formify.Models
{
    public class Role
    {
        public int Id { get; set; }                 // SERIAL
        public string Code { get; set; } = null!;   // 'admin', 'client'
        public string Name { get; set; } = null!;   // 'Администратор'
        public string? Description { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}