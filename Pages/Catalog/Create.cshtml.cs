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
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public void OnGet()
        {
            Input.IsActive = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                Input.ImageData = ms.ToArray();
                Input.ImageName = ImageFile.FileName;
                Input.ImageContentType = ImageFile.ContentType;
            }

            Input.CreateDate = DateTime.UtcNow;
            Input.UpdateDate = DateTime.UtcNow;

            _context.Products.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("Manage");
        }
    }
}
