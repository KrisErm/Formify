using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public class ItemVm
        {
            public long CartItemId { get; set; }
            public long ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal => Price * Quantity;
        }

        public List<ItemVm> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToPage("/Account/Login");

            await LoadCartAsync(userId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostIncreaseAsync(long cartItemId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

            if (item != null)
            {
                item.Quantity += 1;
                item.Cart!.UpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(long cartItemId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

            if (item != null)
            {
                item.Quantity -= 1;
                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Cart!.UpdateDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(long cartItemId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // ----------------- вспомогательные методы -----------------

        private long? GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(idStr, out var id) ? id : null;
        }

        private async Task LoadCartAsync(long userId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    Items = new List<ItemVm>();
                    Total = 0;
                    return;
                }

                Items = cart.Items
                    .Where(i => i.Product != null)
                    .Select(i => new ItemVm
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product!.Name,
                        Price = i.Product.Price,
                        Quantity = i.Quantity
                    })
                    .ToList();

                Total = Items.Sum(i => i.LineTotal);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить корзину.";
                Console.WriteLine(ex);
            }
        }
    }
}
