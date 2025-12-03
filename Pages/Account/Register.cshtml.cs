using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;

        public RegisterModel(AppDbContext context)
        {
            _context = context;
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "Имя")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Display(Name = "Телефон")]
            public string? Phone { get; set; }

            [Required]
            [MinLength(4)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [Required]
            [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
            [Display(Name = "Подтверждение пароля")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var exists = await _context.Users.AnyAsync(u => u.Email == Input.Email);
            if (exists)
            {
                ErrorMessage = "Пользователь с таким email уже зарегистрирован.";
                return Page();
            }

            var clientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "client");
            if (clientRole == null)
            {
                ErrorMessage = "Не найдена роль 'client' в таблице roles.";
                return Page();
            }

            var user = new User
            {
                RoleId = clientRole.Id,
                Name = Input.Name,
                Email = Input.Email,
                Phone = Input.Phone,
                PasswordHash = Input.Password, // для учебного проекта — без шифрования
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // сразу логиним пользователя
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, clientRole.Code)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToPage("/Index");
        }
    }
}
