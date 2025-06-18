using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using CoreLayer;
using Common.DTOs;
using CoreLayer.Services;
using Common;

namespace VinylVaultWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public RegisterDTO Input { get; set; }

        public string? ErrorMessage { get; set; }
        public string? EmailError { get; set; }

        public SignUpModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[DEBUG] SignUp: Invalid model state");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"[ERROR] Field '{key}' - {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            if (await _userService.EmailExists(Input.Email))
            {
                EmailError = "This email is already registered.";
                return Page();
            }

            var newUser = new Person
            {
                UserId = Guid.NewGuid(),
                Email = Input.Email,
                PasswordHash = PasswordHasher.Hash(Input.Password),
                FullName = Input.FullName,
                Address = Input.Address,
                Role = "User"
            };

            Guid? insertedUserId = await _userService.RegisterUser(newUser);

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

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            return RedirectToPage("/Index");
        }

    }
}
