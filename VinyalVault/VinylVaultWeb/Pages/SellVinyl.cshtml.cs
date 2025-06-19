// SellVinylModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CoreLayer.Services;
using Common;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using CoreLayer;

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

        [BindProperty] 
        public decimal Price { get; set; }

        public string? SuccessMessage { get; set; }

        public SellVinylModel(IUserService userService, IVinylService vinylService)
        {
            _userService = userService;
            _vinylService = vinylService;
        }

        public IActionResult OnGet(string albumId, string title, string artist)
        {
            if (string.IsNullOrEmpty(albumId)
             || string.IsNullOrEmpty(title)
             || string.IsNullOrEmpty(artist))
                return RedirectToPage("/Marketplace");

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

            var email = HttpContext.User.Identity?.Name!;
            var user = await _userService.GetUserByEmail(email);
            if (user == null || user.Role != "Seller")
                return RedirectToPage("/LogIn");

            var vinylsDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                                               "wwwroot/images/vinyls");
            Directory.CreateDirectory(vinylsDirectory);
            var imagePath = Path.Combine(vinylsDirectory, TrackImage.FileName);
            using var fs = new FileStream(imagePath, FileMode.Create);
            await TrackImage.CopyToAsync(fs);

            var newVinyl = new Vinyl
            {
                AlbumId = AlbumId,
                Title = VinylTitle,
                Artist = VinylArtist,
                Condition = Condition,
                Description = Description,
                ImagePath = $"/images/vinyls/{TrackImage.FileName}",
                SellerEmail = user.Email,
                Status = "Available",
                Price = Price            
            };

            var isSaved = await _vinylService.UploadVinyl(newVinyl);
            if (isSaved)
                return RedirectToPage(new { albumId = AlbumId, title = VinylTitle, artist = VinylArtist });

            ModelState.AddModelError(string.Empty, "Failed to list vinyl.");
            return Page();
        }
    }
}
