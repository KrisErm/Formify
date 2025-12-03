using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.CustomRequests
{
    [Authorize(Roles = "admin")]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        public long RequestId { get; set; }
        public string UserName { get; set; } = "";
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? CommentUser { get; set; }

        public class ItemRow
        {
            public string TypeName { get; set; } = "";
            public int Quantity { get; set; }
            public string? Note { get; set; }
        }

        public class ImageVm
        {
            public string Src { get; set; } = "";
        }

        public List<ItemRow> Items { get; set; } = new();
        public List<ImageVm> Images { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();

        public class InputModel
        {
            public decimal? FinalPrice { get; set; }
            public string? CommentAdmin { get; set; }
            public int StatusId { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            await LoadDataAsync(id);
            if (!string.IsNullOrEmpty(ErrorMessage))
                return Page();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            var request = await _context.CustomRequests
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                ErrorMessage = "Заявка не найдена.";
                return Page();
            }

            request.FinalPrice = Input.FinalPrice;
            request.CommentAdmin = Input.CommentAdmin;
            request.StatusId = Input.StatusId;
            request.UpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            SuccessMessage = "Изменения сохранены.";
            await LoadDataAsync(id);

            return Page();
        }

        private async Task LoadDataAsync(long id)
        {
            var request = await _context.CustomRequests
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Items)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                ErrorMessage = "Заявка не найдена.";
                return;
            }

            RequestId = request.Id;
            UserName = request.User?.Name ?? "";
            UserEmail = request.User?.Email;
            UserPhone = request.User?.Phone;
            CommentUser = request.CommentUser;

            Items = request.Items
                .Select(i => new ItemRow
                {
                    TypeName = i.TypeName,
                    Quantity = i.Quantity,
                    Note = i.Note
                })
                .ToList();

            Images = request.Images
                .Select(img => new ImageVm
                {
                    Src = $"data:{img.ImageContentType};base64,{Convert.ToBase64String(img.ImageData)}"
                })
                .ToList();

            var statuses = await _context.RequestStatuses
                .OrderBy(s => s.Id)
                .ToListAsync();

            Statuses = statuses.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = s.Id == request.StatusId
            }).ToList();

            Input = new InputModel
            {
                FinalPrice = request.FinalPrice,
                CommentAdmin = request.CommentAdmin,
                StatusId = request.StatusId
            };
        }
    }
}
