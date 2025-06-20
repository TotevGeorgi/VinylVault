using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace VinylVaultWeb.Pages
{
    public class VinylDetailsModel : PageModel
    {
        private readonly ISpotifyAlbumService _albumService;
        private readonly IWishlistService _wishlistService;
        private readonly IVinylService _vinylService;

        public SpotifyAlbumDetails Album { get; set; }
        public bool IsAvailable { get; set; }

        public VinylDetailsModel(ISpotifyAlbumService albumService, IWishlistService wishlistService, IVinylService vinylService)
        {
            _albumService = albumService;
            _wishlistService = wishlistService;
            _vinylService = vinylService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {

            if (string.IsNullOrEmpty(id))
                return RedirectToPage("/Marketplace");

            Album = await _albumService.GetAlbumDetailsAsync(id);
            if (Album == null)
            {
                return NotFound();
            }

            IsAvailable = await _vinylService.IsAlbumAvailable(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAddToWishlistAsync(string albumId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage(new { id = albumId });
            }

            bool success = await _wishlistService.AddSpotifyAlbumToWishlist(userId, albumId);

            TempData[success ? "Success" : "Error"] = success ? "Album added to wishlist." : "Album already in wishlist.";
            return RedirectToPage(new { id = albumId });
        }

    }
}
