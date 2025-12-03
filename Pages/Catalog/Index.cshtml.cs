using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // ---------- показ каталога для клиента ----------
        public async Task OnGetAsync()
        {
            try
            {
                Products = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить каталог. Проверьте подключение к базе данных.";
                Console.WriteLine(ex);
                Products = new List<Product>();
            }
        }

        // ---------- добавление товара в корзину ----------
        public async Task<IActionResult> OnPostAddToCartAsync(long productId)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToPage("/Account/Login");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Находим или создаём корзину пользователя
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Formify.Models.Cart   // <- здесь явно указываем класс
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (item == null)
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(item);
            }
            else
            {
                item.Quantity += 1;
            }

            cart.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // возвращаемся на страницу каталога
            return RedirectToPage();
        }
    }
}
