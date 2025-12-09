using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formify.Pages.CustomRequests
{
    [Authorize(Roles = "admin")]
    public class ManageModel : PageModel
    {
        private readonly AppDbContext _context;

        public ManageModel(AppDbContext context)
        {
            _context = context;
        }

        public class RequestRow
        {
            public long Id { get; set; }
            public string UserName { get; set; } = "";
            public string StatusName { get; set; } = "";
            public decimal? FinalPrice { get; set; }
            public DateTime CreateDate { get; set; }
            public List<ItemVm> Items { get; set; } = new();
            public List<ImageVm> Images { get; set; } = new();
            public string? CommentUser { get; set; }
        }

        public class ItemVm
        {
            public string TypeName { get; set; } = "";
            public int Quantity { get; set; }
            public string? Note { get; set; }
        }

        public class ImageVm
        {
            public string Src { get; set; } = "";
            public bool IsMain { get; set; }
        }

        // Свойства для фильтрации
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public List<RequestRow> Requests { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public List<string> AvailableStatuses { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Получаем все заявки
                var query = _context.CustomRequests
                    .Include(r => r.User)
                    .Include(r => r.Status)
                    .Include(r => r.Items)
                    .Include(r => r.Images)
                    .AsQueryable();

                // Применяем фильтр по дате "с"
                if (StartDate.HasValue)
                {
                    query = query.Where(r => r.CreateDate.Date >= StartDate.Value.Date);
                }

                // Применяем фильтр по дате "по"
                if (EndDate.HasValue)
                {
                    // Добавляем 1 день, чтобы включить весь последний день
                    var endDateNextDay = EndDate.Value.Date.AddDays(1);
                    query = query.Where(r => r.CreateDate < endDateNextDay);
                }

                // Применяем фильтр по статусу
                if (!string.IsNullOrEmpty(StatusFilter))
                {
                    query = query.Where(r => r.Status.Name == StatusFilter);
                }

                // Сортируем по дате (новые сверху)
                query = query.OrderByDescending(r => r.CreateDate);

                var requests = await query.ToListAsync();

                // Получаем список доступных статусов для фильтра
                AvailableStatuses = await _context.RequestStatuses
                    .Select(s => s.Name)
                    .Distinct()
                    .ToListAsync();

                // Маппим данные для отображения
                Requests = requests.Select(r => new RequestRow
                {
                    Id = r.Id,
                    UserName = r.User?.Name ?? "Неизвестно",
                    StatusName = r.Status?.Name ?? "Без статуса",
                    FinalPrice = r.FinalPrice,
                    CreateDate = r.CreateDate,
                    Items = r.Items.Select(i => new ItemVm
                    {
                        TypeName = i.TypeName,
                        Quantity = i.Quantity,
                        Note = i.Note
                    }).ToList(),
                    Images = r.Images.Select(img => new ImageVm
                    {
                        Src = $"data:{img.ImageContentType};base64,{Convert.ToBase64String(img.ImageData)}",
                        IsMain = img.IsMain
                    }).ToList(),
                    CommentUser = r.CommentUser
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить заявки: " + ex.Message;
                Console.WriteLine(ex);
            }
        }

        // Метод для сброса фильтров
        public IActionResult OnGetReset()
        {
            StartDate = null;
            EndDate = null;
            StatusFilter = null;
            return RedirectToPage();
        }
    }
}