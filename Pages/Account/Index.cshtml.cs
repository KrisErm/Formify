using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
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
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }
            public int StatusId { get; set; } // Добавляем StatusId для изменения статуса
        }

        public class CustomRequestVm
        {
            public long Id { get; set; }
            public string Status { get; set; } = "";
            public decimal? FinalPrice { get; set; }
            public DateTime CreateDate { get; set; }
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }
            public int StatusId { get; set; } // Добавляем StatusId для изменения статуса
        }

        public UserInfoVm UserInfo { get; set; } = new();
        public List<OrderVm> Orders { get; set; } = new();
        public List<CustomRequestVm> CustomRequests { get; set; } = new();
        public List<OrderStatus> OrderStatuses { get; set; } = new(); // Список статусов для заказов
        public List<RequestStatus> RequestStatuses { get; set; } = new(); // Список статусов для заявок
        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Account/Login");

            IsAdmin = User.IsInRole("admin");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToPage("/Account/Login");

            UserInfo = new UserInfoVm
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };

            // Для админа загружаем все статусы
            if (IsAdmin)
            {
                OrderStatuses = await _context.OrderStatuses.ToListAsync();
                RequestStatuses = await _context.RequestStatuses.ToListAsync();
            }

            // Заказы
            var ordersQuery = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.DeliveryMethod);

            if (IsAdmin)
            {
                Orders = await ordersQuery
                    .Include(o => o.User)
                    .OrderByDescending(o => o.CreateDate)
                    .Select(o => new OrderVm
                    {
                        Id = o.Id,
                        Status = o.Status != null ? o.Status.Name : "",
                        StatusId = o.StatusId,
                        DeliveryMethod = o.DeliveryMethod != null ? o.DeliveryMethod.Name : "",
                        TotalAmount = o.TotalAmount,
                        CreateDate = o.CreateDate,
                        UserName = o.User != null ? o.User.Name : "",
                        UserEmail = o.User != null ? o.User.Email : ""
                    })
                    .ToListAsync();
            }
            else
            {
                Orders = await ordersQuery
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreateDate)
                    .Select(o => new OrderVm
                    {
                        Id = o.Id,
                        Status = o.Status != null ? o.Status.Name : "",
                        StatusId = o.StatusId,
                        DeliveryMethod = o.DeliveryMethod != null ? o.DeliveryMethod.Name : "",
                        TotalAmount = o.TotalAmount,
                        CreateDate = o.CreateDate
                    })
                    .ToListAsync();
            }

            // Кастомные заявки
            var customRequestsQuery = _context.CustomRequests
                .Include(r => r.Status);

            if (IsAdmin)
            {
                CustomRequests = await customRequestsQuery
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreateDate)
                    .Select(r => new CustomRequestVm
                    {
                        Id = r.Id,
                        Status = r.Status != null ? r.Status.Name : "",
                        StatusId = r.StatusId,
                        FinalPrice = r.FinalPrice,
                        CreateDate = r.CreateDate,
                        UserName = r.User != null ? r.User.Name : "",
                        UserEmail = r.User != null ? r.User.Email : ""
                    })
                    .ToListAsync();
            }
            else
            {
                CustomRequests = await customRequestsQuery
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreateDate)
                    .Select(r => new CustomRequestVm
                    {
                        Id = r.Id,
                        Status = r.Status != null ? r.Status.Name : "",
                        StatusId = r.StatusId,
                        FinalPrice = r.FinalPrice,
                        CreateDate = r.CreateDate
                    })
                    .ToListAsync();
            }

            return Page();
        }

        // Обработчик для изменения статуса заказа
        public async Task<IActionResult> OnPostUpdateOrderStatusAsync(long orderId, int statusId)
        {
            if (!User.IsInRole("admin"))
                return Forbid();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            var status = await _context.OrderStatuses.FindAsync(statusId);
            if (status == null)
                return BadRequest("Invalid status");

            order.StatusId = statusId;
            order.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, newStatus = status.Name });
        }

        // Обработчик для изменения статуса кастомной заявки
        public async Task<IActionResult> OnPostUpdateRequestStatusAsync(long requestId, int statusId)
        {
            if (!User.IsInRole("admin"))
                return Forbid();

            var request = await _context.CustomRequests.FindAsync(requestId);
            if (request == null)
                return NotFound();

            var status = await _context.RequestStatuses.FindAsync(statusId);
            if (status == null)
                return BadRequest("Invalid status");

            request.StatusId = statusId;
            request.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, newStatus = status.Name });
        }
    }
}