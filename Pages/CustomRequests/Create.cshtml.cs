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
            Console.WriteLine("=== OnPostAsync вызван ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ErrorMessage = $"Ошибки формы: {errors}";
                Console.WriteLine($"ModelState ошибки: {errors}");
                return Page();
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"userIdStr: '{userIdStr}'");
            if (!long.TryParse(userIdStr, out var userId))
            {
                ErrorMessage = "Ошибка авторизации. Перезайдите в аккаунт.";
                return Page();
            }

            var statusNew = await _context.RequestStatuses.FirstOrDefaultAsync(s => s.Code == "new");
            Console.WriteLine($"statusNew найден: {statusNew != null}");
            if (statusNew == null)
            {
                ErrorMessage = "У нас нет статуса 'new'. Обратитесь к администратору.";
                return Page();
            }

            try
            {
                var request = new CustomRequest { /* ... ваш код ... */ };
                _context.CustomRequests.Add(request);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Заявка создана: ID={request.Id}");

                // item и images тоже остаются
                await _context.SaveChangesAsync();
                SuccessMessage = "✅ Заявка отправлена! После обработки администратор выставит цену и статус.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка БД: {ex.Message}";
                Console.WriteLine($"EXCEPTION: {ex}");
            }

            Input = new InputModel();
            Images = null;
            return Page();
        }

    }
}
