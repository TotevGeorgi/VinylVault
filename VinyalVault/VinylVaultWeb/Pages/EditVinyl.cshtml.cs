using System.Collections.Generic;
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
    public class EditVinylModel : PageModel
    {
        private readonly IVinylService _vinylService;
        private readonly IUserService _userService;

        public EditVinylModel(IVinylService vinylService, IUserService userService)
        {
            _vinylService = vinylService;
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Artist { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public string Condition { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public IFormFile? TrackImage { get; set; }

        public string ImagePath { get; set; }

        public string? SuccessMessage { get; set; }

        public List<SelectListItem> ConditionOptions { get; } = new()
    {
      new SelectListItem("Mint","Mint"),
      new SelectListItem("Near Mint","Near Mint"),
      new SelectListItem("Good","Good"),
      new SelectListItem("Fair","Fair")
    };

        public async Task<IActionResult> OnGetAsync()
        {
            var vinyl = await _vinylService.GetVinylById(Id);
            if (vinyl == null) return NotFound();

            Title = vinyl.Title;
            Artist = vinyl.Artist;
            Price = vinyl.Price;
            Condition = vinyl.Condition;
            Description = vinyl.Description;
            ImagePath = vinyl.ImagePath;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var vinyl = await _vinylService.GetVinylById(Id);
            if (vinyl == null) return NotFound();

            vinyl.Price = Price;
            vinyl.Condition = Condition;
            vinyl.Description = Description;

            if (TrackImage != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/vinyls");
                Directory.CreateDirectory(folder);
                var file = Path.Combine(folder, TrackImage.FileName);
                using var fs = new FileStream(file, FileMode.Create);
                await TrackImage.CopyToAsync(fs);
                vinyl.ImagePath = $"/images/vinyls/{TrackImage.FileName}";
            }

            var ok = await _vinylService.UpdateVinyl(vinyl);
            if (ok)
            {
                SuccessMessage = "Listing updated!";
                return RedirectToPage(new { id = Id });
            }

            ModelState.AddModelError("", "Failed to update.");
            return Page();
        }
    }
}
