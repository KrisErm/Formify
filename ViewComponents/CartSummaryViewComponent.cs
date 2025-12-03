using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Formify.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CartSummaryViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userIdStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (long.TryParse(userIdStr, out var userId))
                {
                    var cart = await _context.Carts
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart != null)
                    {
                        count = cart.Items.Sum(i => i.Quantity);
                    }
                }
            }

            // Вьюха получит просто число (int)
            return View(count);
        }
    }
}
