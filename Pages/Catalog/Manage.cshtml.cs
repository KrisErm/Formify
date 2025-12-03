using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Catalog
{
    [Authorize(Roles = "admin")]
    public class ManageModel : PageModel
    {
        private readonly AppDbContext _context;

        public ManageModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Products = await _context.Products
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить данные каталога.";
                Console.WriteLine(ex);
            }
        }

        // переключаем IsActive
        public async Task<IActionResult> OnPostToggleAsync(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return RedirectToPage();

            product.IsActive = !product.IsActive;
            product.UpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
