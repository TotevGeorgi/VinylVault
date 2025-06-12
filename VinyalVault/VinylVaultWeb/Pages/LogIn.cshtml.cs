using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Common.DTOs;
using CoreLayer.Services;

namespace VinylVaultWeb.Pages
{
    public class LogInModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; } = false;

        public string? ErrorMessage { get; set; }

        public LogInModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userService.AuthenticateUser(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password. Please try again.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),  
                new Claim(ClaimTypes.Role, user.Role ?? "User"),  
                new Claim("FullName", user.FullName ?? "") 
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToPage("/Index");
        }
    }
}
