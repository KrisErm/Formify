using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Account
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public class UserInfoVm
        {
            public string Name { get; set; } = "";
            public string? Email { get; set; }
            public string? Phone { get; set; }
        }

        public class OrderVm
        {
            public long Id { get; set; }
            public string Status { get; set; } = "";
            public string DeliveryMethod { get; set; } = "";
            public decimal? TotalAmount { get; set; }
            public DateTime CreateDate { get; set; }
        }

        public class CustomRequestVm
        {
            public long Id { get; set; }
            public string Status { get; set; } = "";
            public decimal? FinalPrice { get; set; }
            public DateTime CreateDate { get; set; }
        }

        public UserInfoVm UserInfo { get; set; } = new();
        public List<OrderVm> Orders { get; set; } = new();
        public List<CustomRequestVm> CustomRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Account/Login");

            // инфо по пользователю
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return RedirectToPage("/Account/Login");

            UserInfo = new UserInfoVm
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };

            // заказы
            Orders = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.DeliveryMethod)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreateDate)
                .Select(o => new OrderVm
                {
                    Id = o.Id,
                    Status = o.Status != null ? o.Status.Name : "",
                    DeliveryMethod = o.DeliveryMethod != null ? o.DeliveryMethod.Name : "",
                    TotalAmount = o.TotalAmount,
                    CreateDate = o.CreateDate
                })
                .ToListAsync();

            // кастомные за€вки
            CustomRequests = await _context.CustomRequests
                .Include(r => r.Status)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreateDate)
                .Select(r => new CustomRequestVm
                {
                    Id = r.Id,
                    Status = r.Status != null ? r.Status.Name : "",
                    FinalPrice = r.FinalPrice,
                    CreateDate = r.CreateDate
                })
                .ToListAsync();

            return Page();
        }
    }
}
