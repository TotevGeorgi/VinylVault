using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Common;
using Common.DTOs;
using CoreLayer.Services;
using CoreLayer;
using Microsoft.Extensions.Logging;

namespace VinylVaultWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IRegistrationService _registrationService;
        private readonly IPasswordHasher _passwordHasher;

        [BindProperty]
        public RegisterDTO Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? EmailError { get; set; }

        public SignUpModel(
            IRegistrationService registrationService,
            IPasswordHasher passwordHasher)
        {
            _registrationService = registrationService;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _registrationService.EmailExists(Input.Email))
            {
                EmailError = "This email is already registered.";
                return Page();
            }

            var newUser = new Person
            {
                UserId = Guid.NewGuid(),
                Email = Input.Email,
                PasswordHash = _passwordHasher.Hash(Input.Password),
                FullName = Input.FullName,
                Address = Input.Address,
                Role = "User"
            };

            Guid? insertedUserId = await _registrationService.RegisterUser(newUser);
            if (insertedUserId == null)
            {
                ErrorMessage = "Failed to create account.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Input.Email),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("FullName", Input.FullName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, insertedUserId.Value.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToPage("/Index");
        }
    }
}
