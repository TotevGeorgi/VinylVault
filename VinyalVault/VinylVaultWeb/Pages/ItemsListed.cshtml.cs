using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CoreLayer;
using CoreLayer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace VinylVaultWeb.Pages
{
    [Authorize(Roles = "Seller")]
    public class ListedItemsModel : PageModel
    {
        private readonly IVinylService _vinylService;
        private readonly IUserService _userService;

        public ListedItemsModel(IVinylService vinylService, IUserService userService)
        {
            _vinylService = vinylService;
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "active";         

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "dateDesc";    

        public List<Vinyl> ActiveVinyls { get; set; } = new();
        public List<Vinyl> SoldVinyls { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmail(email);
            if (user?.Role != "Seller")
                return RedirectToPage("/LogIn");

            var all = await _vinylService.GetVinylsBySeller(user.Email);

            ActiveVinyls = all.Where(v => v.Status != "Sold").ToList();
            SoldVinyls = all.Where(v => v.Status == "Sold").ToList();

            void ApplySort(List<Vinyl> list)
            {
                switch (SortBy)
                {
                    case "priceAsc":
                        list.Sort((a, b) => a.Price.CompareTo(b.Price));
                        break;
                    case "priceDesc":
                        list.Sort((a, b) => b.Price.CompareTo(a.Price));
                        break;
                    case "dateDesc":
                    default:
                        list.Sort((a, b) => b.DateAdded.CompareTo(a.DateAdded));
                        break;
                }
            }

            ApplySort(ActiveVinyls);
            ApplySort(SoldVinyls);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var ok = await _vinylService.DeleteVinyl(id);
            SuccessMessage = ok
                ? "Vinyl removed successfully."
                : "Failed to remove vinyl.";
            return RedirectToPage(new { Tab, SortBy });
        }
    }
}
