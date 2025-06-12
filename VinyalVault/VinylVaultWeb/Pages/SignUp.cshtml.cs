using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CoreLayer.Services;
using Common.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Common;

namespace VinylVaultWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public Person User { get; set; }

        public string? ErrorMessage { get; set; }
        public string? EmailError { get; set; }

        public SignUpModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (await _userService.EmailExists(User.Email))  // Ensure EmailExists is awaited
            {
                EmailError = "This email is already registered.";
                return Page();
            }

            bool isRegistered = await _userService.RegisterUser(User);  // Await the asynchronous call

            if (!isRegistered)
            {
                ErrorMessage = "Failed to create account.";
                return Page();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, User.Email),
        new Claim(ClaimTypes.Email, User.Email)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToPage("/Index");
        }

    }
}