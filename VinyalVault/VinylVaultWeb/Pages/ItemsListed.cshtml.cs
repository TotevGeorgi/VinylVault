using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CoreLayer.Services;
using Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using CoreLayer;
using Microsoft.AspNetCore.Authorization;

namespace VinylVaultWeb.Pages
{
    [Authorize(Roles = "Seller")]
    public class ListedItemsModel : PageModel
    {
        private readonly IVinylService _vinylService;
        private readonly IUserService _userService;

        public List<Vinyl> Vinyls { get; set; }
        public string? SuccessMessage { get; set; }

        public ListedItemsModel(IVinylService vinylService, IUserService userService)
        {
            _vinylService = vinylService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? email = HttpContext.User.Identity?.Name;
            var user = await _userService.GetUserByEmail(email);

            if (user == null || user.Role != "Seller")
            {
                return RedirectToPage("/LogIn");
            }

            Vinyls = await _vinylService.GetVinylsBySeller(user.Email);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool isDeleted = await _vinylService.DeleteVinyl(id);

            if (isDeleted)
            {
                SuccessMessage = "Vinyl removed successfully.";
            }
            else
            {
                SuccessMessage = "Failed to remove vinyl.";
            }

            return RedirectToPage();
        }
    }
}
