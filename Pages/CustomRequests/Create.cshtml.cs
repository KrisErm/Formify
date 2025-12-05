using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.CustomRequests
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        public class InputModel
        {
            [Required]
            public string TypeName { get; set; } = string.Empty;

            [Required]
            [Range(1, 1000)]
            public int Quantity { get; set; } = 1;

            public string? Note { get; set; }
            public string? Comment { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public List<IFormFile>? Images { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }
            var statusNew = await _context.RequestStatuses
                .FirstOrDefaultAsync(s => s.Code == "new");

            if (statusNew == null)
            {
                // Создаем статус, если его нет
                statusNew = new RequestStatus
                {
                    Code = "new",
                    Name = "Новая",
                };
                _context.RequestStatuses.Add(statusNew);
                await _context.SaveChangesAsync();
            }

            var request = new CustomRequest
            {
                UserId = userId,
                StatusId = statusNew.Id,
                CommentUser = Input.Comment,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            _context.CustomRequests.Add(request);
            await _context.SaveChangesAsync();

            var item = new CustomRequestItem
            {
                RequestId = request.Id,
                TypeName = Input.TypeName,
                Quantity = Input.Quantity,
                Note = Input.Note
            };
            _context.CustomRequestItems.Add(item);

            if (Images != null && Images.Any())
            {
                bool first = true;
                foreach (var file in Images)
                {
                    if (file.Length <= 0) continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    var img = new CustomRequestImage
                    {
                        RequestId = request.Id,
                        ImageData = ms.ToArray(),
                        ImageName = file.FileName,
                        ImageContentType = file.ContentType,
                        IsMain = first,
                        CreateDate = DateTime.UtcNow
                    };
                    first = false;

                    _context.CustomRequestImages.Add(img);
                }
            }

            await _context.SaveChangesAsync();

            SuccessMessage = "Заявка отправлена. После обработки администратор выставит цену и статус.";
            // очистим форму
            Input = new InputModel();
            Images = null;

            return Page();
        }
    }
}