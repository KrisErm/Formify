using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Formify.Pages.Order
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Свойства для фильтрации
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Дата с")]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Дата по")]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Статус")]
        public int? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Способ доставки")]
        public int? DeliveryMethodFilter { get; set; }

        // Коллекции для выпадающих списков
        public List<OrderStatus> AvailableStatuses { get; set; } = new();
        public List<DeliveryMethod> AvailableDeliveryMethods { get; set; } = new();
        public List<OrderViewModel> Orders { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class OrderViewModel
        {
            public long Id { get; set; }
            public string UserName { get; set; } = string.Empty; // Это будет заполнено из User.Name
            public string StatusName { get; set; } = string.Empty;
            public string DeliveryMethodName { get; set; } = string.Empty;
            public decimal? TotalAmount { get; set; }
            public decimal? DeliveryPrice { get; set; }
            public string? DeliveryCity { get; set; }
            public string? DeliveryAddress { get; set; }
            public DateTime CreateDate { get; set; }
            public string? Comment { get; set; }
            public bool HasCustomRequest { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Получаем данные для фильтров
                AvailableStatuses = await _context.OrderStatuses
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                AvailableDeliveryMethods = await _context.DeliveryMethods
                    .OrderBy(d => d.Id)
                    .ToListAsync();

                // Начинаем запрос с фильтрацией НЕ кастомных заказов
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Status)
                    .Include(o => o.DeliveryMethod)
                    .Where(o => o.CustomRequestId == null) // Только НЕ кастомные заказы
                    .AsQueryable();

                // Применяем фильтры
                if (StartDate.HasValue)
                {
                    query = query.Where(o => o.CreateDate >= StartDate.Value.Date);
                }

                if (EndDate.HasValue)
                {
                    var endDateWithTime = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(o => o.CreateDate <= endDateWithTime);
                }

                if (StatusFilter.HasValue && StatusFilter > 0)
                {
                    query = query.Where(o => o.StatusId == StatusFilter.Value);
                }

                if (DeliveryMethodFilter.HasValue && DeliveryMethodFilter > 0)
                {
                    query = query.Where(o => o.DeliveryMethodId == DeliveryMethodFilter.Value);
                }

                // Сортируем по дате создания (сначала новые)
                query = query.OrderByDescending(o => o.CreateDate);

                // Получаем данные и преобразуем в ViewModel
                var orders = await query.ToListAsync();

                Orders = orders.Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    // Используем Name вместо UserName
                    UserName = o.User?.Name ?? "Неизвестно",
                    StatusName = o.Status?.Name ?? "Неизвестно",
                    DeliveryMethodName = o.DeliveryMethod?.Name ?? "Неизвестно",
                    TotalAmount = o.TotalAmount,
                    DeliveryPrice = o.DeliveryPrice,
                    DeliveryCity = o.DeliveryCity,
                    DeliveryAddress = o.DeliveryAddress,
                    CreateDate = o.CreateDate,
                    Comment = o.Comment,
                    HasCustomRequest = o.CustomRequestId.HasValue
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке заказов: {ex.Message}";
            }
        }

        // Обработчик сброса фильтров
        public IActionResult OnGetReset()
        {
            StartDate = null;
            EndDate = null;
            StatusFilter = null;
            DeliveryMethodFilter = null;
            return RedirectToPage();
        }
    }
}