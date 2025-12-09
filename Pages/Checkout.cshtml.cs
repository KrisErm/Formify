using Microsoft.AspNetCore.Mvc.RazorPages;
using Formify.Data;
using Formify.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Formify.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly AppDbContext _context;
        public CheckoutModel(AppDbContext context) => _context = context;

        public string TotalDisplay { get; set; } = "0";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";
        public string DebugInfo { get; set; } = "";

        public void OnGet() => TotalDisplay = Request.Query["total"].ToString() ?? "0";

        public void OnPost()
        {
            TotalDisplay = Request.Query["total"].ToString() ?? "0";
            FullName = Request.Form["FullName"];
            Phone = Request.Form["Phone"];
            Address = Request.Form["Address"];

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Заполните все обязательные поля!";
                return;
            }

            try
            {
                var status = _context.OrderStatuses.FirstOrDefault(s => s.Code == "new");
                var delivery = _context.DeliveryMethods.FirstOrDefault(d => d.Code == "courier");

                DebugInfo = $"StatusId={status?.Id}({status?.Name}), DeliveryId={delivery?.Id}({delivery?.Name})";

                var nowUtc = DateTime.UtcNow;  // ← UTC для PostgreSQL!

                var order = new Order
                {
                    UserId = GetUserId(),
                    StatusId = status?.Id ?? 1,
                    DeliveryMethodId = delivery?.Id ?? 1,
                    TotalAmount = decimal.TryParse(TotalDisplay, out var total) ? total : 0m,
                    DeliveryPrice = 0m,
                    DeliveryFullName = FullName,
                    DeliveryPhone = Phone,
                    DeliveryCity = "Москва",  // ← обязательное поле
                    DeliveryAddress = Address,
                    CreateDate = nowUtc,      // ← UTC!
                    UpdateDate = nowUtc       // ← UTC!
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                SuccessMessage = $"✅ Заказ #{order.Id} успешно оплачен!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && long.TryParse(claim.Value, out var uid) ? uid : 0L;
        }
    }
}
