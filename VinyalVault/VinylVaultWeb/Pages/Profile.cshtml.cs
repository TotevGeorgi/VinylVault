using Common;
using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinylVaultWeb.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IRatingService _ratingService;
        private readonly IVinylService _vinylService;

        [BindProperty(SupportsGet = true)]
        public Person User { get; set; } = new();

        public string? Message { get; set; }

        public List<SellerRatingDTO> SellerRatings { get; set; } = new();

        public List<Vinyl> SellerVinyls { get; set; } = new();

        public ProfileModel(IUserService userService, IRatingService ratingService, IVinylService vinylService)
        {
            _userService = userService;
            _ratingService = ratingService;
            _vinylService = vinylService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/LogIn");

            var userFromDb = await _userService.GetUserByEmail(email);
            if (userFromDb == null) return RedirectToPage("/LogIn");

            User = userFromDb;

            if (User.Role == "Seller")
            {
                SellerRatings = await _ratingService.GetRatingsForSellerAsync(User.Email);
                SellerVinyls = await _vinylService.GetVinylsBySeller(User.Email);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            string? email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/LogIn");

            _userService.UpdateUserProfile(email, User.FullName, User.Address);
            Message = "Profile updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBecomeSellerAsync()
        {
            string? email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/LogIn");

            await _userService.UpgradeToSeller(email);
            return RedirectToPage();
        }
    }
}
