using Common;
using Common.DTOs;
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

        [BindProperty(SupportsGet = true)]
        public Person User { get; set; }

        public string? SuccessMessage { get; set; }

        public ProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()  
        {
            string? email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/LogIn");

            var userFromDb = await _userService.GetUserByEmail(email);  
            if (userFromDb == null) return RedirectToPage("/LogIn");

            User = userFromDb;  
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync() 
        {
            string? email = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/LogIn");

             _userService.UpdateUserProfile(email, User.FullName, User.Address);  
            SuccessMessage = "Profile updated successfully.";
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
