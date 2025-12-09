using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.CustomRequests
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly AppDbContext _context;

        public HistoryModel(AppDbContext context)
        {
            _context = context;
        }

        public class RequestViewModel
        {
            public long Id { get; set; }
            public string TypeName { get; set; } = "";
            public int Quantity { get; set; }
            public string? Note { get; set; }
            public string StatusName { get; set; } = "";
            public decimal? FinalPrice { get; set; }
            public string? CommentAdmin { get; set; }
            public DateTime CreateDate { get; set; }
            public bool HasImages { get; set; }
            public List<ImageViewModel> Images { get; set; } = new();
        }

        public class ImageViewModel
        {
            public string Src { get; set; } = "";
        }

        public List<RequestViewModel> Requests { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out var userId))
                {
                    ErrorMessage = "Ошибка авторизации";
                    return;
                }

                var requests = await _context.CustomRequests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Status)
                    .Include(r => r.Items)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.CreateDate)
                    .ToListAsync();

                Requests = requests.Select(r => new RequestViewModel
                {
                    Id = r.Id,
                    TypeName = r.Items.FirstOrDefault()?.TypeName ?? "Без названия",
                    Quantity = r.Items.FirstOrDefault()?.Quantity ?? 0,
                    Note = r.Items.FirstOrDefault()?.Note,
                    StatusName = r.Status?.Name ?? "Неизвестно",
                    FinalPrice = r.FinalPrice,
                    CommentAdmin = r.CommentAdmin,
                    CreateDate = r.CreateDate,
                    HasImages = r.Images.Any(),
                    Images = r.Images.Select(img => new ImageViewModel
                    {
                        Src = $"data:{img.ImageContentType};base64,{Convert.ToBase64String(img.ImageData)}"
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
            }
        }
    }
}