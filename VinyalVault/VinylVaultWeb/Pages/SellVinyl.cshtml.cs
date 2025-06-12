using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CoreLayer.Services;
using Common;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using CoreLayer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VinylVaultWeb.Pages
{
    [Authorize(Roles = "Seller")]
    public class SellVinylModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IVinylService _vinylService;

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

        public string? SuccessMessage { get; set; }

        public SellVinylModel(IUserService userService, IVinylService vinylService)
        {
            _userService = userService;
            _vinylService = vinylService;
        }

        public IActionResult OnGet(string albumId, string title, string artist)
        {
            if (string.IsNullOrEmpty(albumId) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
            {
                return RedirectToPage("/Marketplace");
            }

            AlbumId = albumId;
            VinylTitle = title;
            VinylArtist = artist;
            return Page();
        }
        public List<SelectListItem> ConditionOptions { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Mint", Text = "Mint" },
            new SelectListItem { Value = "Near Mint", Text = "Near Mint" },
            new SelectListItem { Value = "Good", Text = "Good" },
            new SelectListItem { Value = "Fair", Text = "Fair" }
        };
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(AlbumId) || string.IsNullOrEmpty(VinylTitle) || string.IsNullOrEmpty(VinylArtist))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return Page();
            }

            string? email = HttpContext.User.Identity?.Name;
            var user = await _userService.GetUserByEmail(email);

            if (user == null || user.Role != "Seller")
            {
                return RedirectToPage("/LogIn");
            }

            string vinylsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "vinyls");
            if (!Directory.Exists(vinylsDirectory))
            {
                Directory.CreateDirectory(vinylsDirectory);
            }

            string imagePath = Path.Combine(vinylsDirectory, TrackImage.FileName);
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await TrackImage.CopyToAsync(stream);
            }

            var newVinyl = new Vinyl
            {
                AlbumId = AlbumId,
                Title = VinylTitle,
                Artist = VinylArtist,
                Condition = Condition,
                Description = Description,
                ImagePath = "/images/vinyls/" + TrackImage.FileName,
                SellerEmail = user.Email,
                Status = "Available"
            };

            bool isSaved = await _vinylService.UploadVinyl(newVinyl);

            if (isSaved)
            {
                SuccessMessage = "Vinyl listed for sale successfully.";
                return RedirectToPage(new { albumId = AlbumId, title = VinylTitle, artist = VinylArtist });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to list vinyl.");
                return Page();
            }
        }
    }
}
