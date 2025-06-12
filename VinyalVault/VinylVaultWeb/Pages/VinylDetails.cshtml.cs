using Common.DTOs;
using CoreLayer.Services;
using DataLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinylVaultWeb.Pages
{
    public class VinylDetailsModel : PageModel
    {
        private readonly ISpotifyAlbumService _albumService;

        public SpotifyAlbumDetails Album { get; set; }

        public VinylDetailsModel(ISpotifyAlbumService albumService)
        {
            _albumService = albumService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToPage("/Marketplace");

            Album = await _albumService.GetAlbumDetailsAsync(id);

            if (Album == null)
                return NotFound();

            return Page();
        }
    }
}
