using Common;
using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VinylVaultWeb.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserProfileService _profileService;
        private readonly ISellerService _sellerService;
        private readonly IRatingService _ratingService;
        private readonly IVinylService _vinylService;

        [BindProperty]
        public Person ProfileUser { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public List<SellerRatingDTO> SellerRatings { get; set; } = new();
        public List<Vinyl> SellerVinyls { get; set; } = new();

        public ProfileModel(IAuthenticationService authService, IUserProfileService profileService, ISellerService sellerService, IRatingService ratingService, IVinylService vinylService)
        {
            _authService = authService;
            _profileService = profileService;
            _sellerService = sellerService;
            _ratingService = ratingService;
            _vinylService = vinylService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/LogIn");


            var userFromDb = await _authService.GetUserByEmail(email);
            if (userFromDb == null)
                return RedirectToPage("/LogIn");

            ProfileUser = userFromDb;

            if (ProfileUser.Role == "Seller")
            {
                SellerRatings = await _ratingService.GetRatingsForSellerAsync(email);
                SellerVinyls = await _vinylService.GetVinylsBySeller(email);
            }

            return Page();
        }
        public async Task<IActionResult> OnPostSaveAsync()
        {
            var email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/LogIn");

            var ok = await _profileService
                .UpdateUserProfileAsync(
                    email,
                    ProfileUser.FullName,
                    ProfileUser.Address
                );

            Message = ok
                ? "Profile updated successfully."
                : "An error occurred updating your profile.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBecomeSellerAsync()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/LogIn");

            var upgraded = await _sellerService.UpgradeToSellerAsync(email);
            Message = upgraded
                ? "You’ve been upgraded to a Seller!"
                : "Could not upgrade at this time.";

            return RedirectToPage();
        }
    }
}
