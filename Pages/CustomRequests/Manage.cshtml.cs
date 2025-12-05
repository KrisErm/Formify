using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

        public List<RequestRow> Requests { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var requests = await _context.CustomRequests
                    .Include(r => r.User)
                    .Include(r => r.Status)
                    .Include(r => r.Items)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.CreateDate)
                    .ToListAsync();

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
    }
}