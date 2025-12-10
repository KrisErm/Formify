using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly AppDbContext _context;

        public CheckoutModel(AppDbContext context)
        {
            _context = context;
        }

        public string TotalDisplay { get; set; } = "0";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";

        public int SelectedDeliveryMethodId { get; set; }
        public List<SelectListItem> DeliveryMethods { get; set; } = new();

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public void OnGet()
        {
            TotalDisplay = Request.Query["total"].ToString() ?? "0";
            LoadDeliveryMethods();
        }

        public void OnPost()
        {
            TotalDisplay = Request.Query["total"].ToString() ?? "0";

            FullName = Request.Form["FullName"];
            Phone = Request.Form["Phone"];
            Address = Request.Form["Address"];

            if (int.TryParse(Request.Form["DeliveryMethodId"], out var deliveryId))
                SelectedDeliveryMethodId = deliveryId;
            else
                SelectedDeliveryMethodId = 0;

            LoadDeliveryMethods();

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Address) ||
                SelectedDeliveryMethodId == 0)
            {
                ErrorMessage = "Заполните все поля и выберите способ доставки.";
                return;
            }

            try
            {
                var status = _context.OrderStatuses.FirstOrDefault(s => s.Code == "new");
                var deliveryMethod = _context.DeliveryMethods
                    .FirstOrDefault(d => d.Id == SelectedDeliveryMethodId);

                var nowUtc = DateTime.UtcNow;

                var order = new Formify.Models.Order
                {
                    UserId = GetUserId(),
                    StatusId = status?.Id ?? 1,
                    DeliveryMethodId = SelectedDeliveryMethodId,
                    TotalAmount = decimal.TryParse(TotalDisplay, out var total) ? total : 0m,
                    DeliveryPrice = deliveryMethod?.BasePrice ?? 0m,
                    DeliveryFullName = FullName,
                    DeliveryPhone = Phone,
                    DeliveryCity = "Москва",
                    DeliveryAddress = Address,
                    CreateDate = nowUtc,
                    UpdateDate = nowUtc
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                SuccessMessage = $"Заказ #{order.Id} успешно оплачен!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        private void LoadDeliveryMethods()
        {
            DeliveryMethods = _context.DeliveryMethods
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToList(); // ← без этого и была ошибка преобразования
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
                return userId;

            return 0;
        }
    }
}
