using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using AuthSvc = CoreLayer.Services.IAuthenticationService;
using Common.DTOs;

namespace VinylVaultWeb.Pages
{
    public class LogInModel : PageModel
    {
        private readonly AuthSvc _authenticationService;

        [BindProperty]
        public LoginDTO Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public LogInModel(AuthSvc authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _authenticationService.AuthenticateUser(Input.Email, Input.Password);

            if (user == null || user.UserId == Guid.Empty)
            {
                ErrorMessage = "Invalid email or password. Please try again.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("FullName", user.FullName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties { IsPersistent = Input.RememberMe };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps
            );

            return RedirectToPage("/Index");
        }
    }
}
