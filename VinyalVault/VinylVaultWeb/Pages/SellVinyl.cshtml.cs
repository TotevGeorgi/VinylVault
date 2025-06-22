using System.IO;
using System.Threading.Tasks;
using Common;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VinylVaultWeb.Pages
{
    [Authorize(Roles = "Seller")]
    public class SellVinylModel : PageModel
    {
        private readonly IVinylService _vinylService;
        private readonly IAuthenticationService _authService;

        [BindProperty(SupportsGet = true)]
        public string AlbumId { get; set; }

        [BindProperty]
        public string VinylTitle { get; set; }

        [BindProperty]
        public string VinylArtist { get; set; }

        [BindProperty]
        public string Condition { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public IFormFile TrackImage { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        public string? SuccessMessage { get; set; }

        public SellVinylModel(
            IVinylService vinylService,
            IAuthenticationService authService)
        {
            _vinylService = vinylService;
            _authService = authService;
        }

        public IActionResult OnGet(string albumId, string title, string artist)
        {
            if (string.IsNullOrEmpty(albumId)
             || string.IsNullOrEmpty(title)
             || string.IsNullOrEmpty(artist))
            {
                return RedirectToPage("/Marketplace");
            }

            AlbumId = albumId;
            VinylTitle = title;
            VinylArtist = artist;
            return Page();
        }

        public List<SelectListItem> ConditionOptions { get; } = new()
        {
            new SelectListItem("Mint",      "Mint"),
            new SelectListItem("Near Mint","Near Mint"),
            new SelectListItem("Good",      "Good"),
            new SelectListItem("Fair",      "Fair")
        };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid
             || string.IsNullOrEmpty(AlbumId)
             || string.IsNullOrEmpty(VinylTitle)
             || string.IsNullOrEmpty(VinylArtist))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return Page();
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/LogIn");

            var user = await _authService.GetUserByEmail(email);
            if (user == null || user.Role != "Seller")
                return RedirectToPage("/LogIn");

            var vinylsDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/vinyls"
            );
            Directory.CreateDirectory(vinylsDirectory);

            var fileName = Path.GetFileName(TrackImage.FileName);
            var imagePath = Path.Combine(vinylsDirectory, fileName);
            using var fs = new FileStream(imagePath, FileMode.Create);
            await TrackImage.CopyToAsync(fs);

            var newVinyl = new Vinyl
            {
                AlbumId = AlbumId,
                Title = VinylTitle,
                Artist = VinylArtist,
                Condition = Condition,
                Description = Description,
                ImagePath = $"/images/vinyls/{fileName}",
                SellerEmail = user.Email,
                Status = "Available",
                Price = Price
            };

            var isSaved = await _vinylService.UploadVinyl(newVinyl);
            if (isSaved)
            {
                SuccessMessage = "Your vinyl has been listed!";
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Failed to list your vinyl. Please try again.");
            return Page();
        }
    }
}
