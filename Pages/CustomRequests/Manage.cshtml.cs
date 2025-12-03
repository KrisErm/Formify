using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formify.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.CustomRequests
{
    [Authorize(Roles = "admin")]
    public class ManageModel : PageModel
    {
        private readonly AppDbContext _context;

        public ManageModel(AppDbContext context)
        {
            _context = context;
        }

        public class RequestRow
        {
            public long Id { get; set; }
            public string UserName { get; set; } = "";
            public string StatusName { get; set; } = "";
            public decimal? FinalPrice { get; set; }
            public DateTime CreateDate { get; set; }
        }

        public List<RequestRow> Requests { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Requests = await _context.CustomRequests
                    .Include(r => r.User)
                    .Include(r => r.Status)
                    .OrderByDescending(r => r.CreateDate)
                    .Select(r => new RequestRow
                    {
                        Id = r.Id,
                        UserName = r.User!.Name,
                        StatusName = r.Status!.Name,
                        FinalPrice = r.FinalPrice,
                        CreateDate = r.CreateDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить заявки.";
                Console.WriteLine(ex);
            }
        }
    }
}
