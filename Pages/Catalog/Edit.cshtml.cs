using System;
using System.IO;
using System.Threading.Tasks;
using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Formify.Pages.Catalog
{
    [Authorize(Roles = "admin")]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Input { get; set; } = null!;

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            Input = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            if (id != Input.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = Input.Name;
            product.Description = Input.Description;
            product.Price = Input.Price;
            product.IsActive = Input.IsActive;
            product.UpdateDate = DateTime.UtcNow;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageName = ImageFile.FileName;
                product.ImageContentType = ImageFile.ContentType;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
