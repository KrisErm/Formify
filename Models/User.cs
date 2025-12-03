
using System;
using System.Collections.Generic;

namespace Formify.Models
{
    public class User
    {
        public long Id { get; set; }            // BIGSERIAL
        public int RoleId { get; set; }

        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        public Role? Role { get; set; }
    }
}
