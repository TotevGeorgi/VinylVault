using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace VinylVaultWeb.Pages
{
    [Authorize]
    public class WishlistModel : PageModel
    {
        private readonly IWishlistService _wishlistService;
        private readonly ISpotifyAlbumService _spotifyAlbumService;
        private readonly IVinylService _vinylService;

        public List<AlbumAvailability> WishlistAlbums { get; set; } = new();

        public WishlistModel(IWishlistService wishlistService, ISpotifyAlbumService spotifyAlbumService, IVinylService vinylService)
        {
            _wishlistService = wishlistService;
            _spotifyAlbumService = spotifyAlbumService;
            _vinylService = vinylService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/LogIn");
            }

            var albumIds = await _wishlistService.GetAlbumIdsInWishlist(userId);

            var tasks = albumIds.Select(async albumId =>
            {
                var albumDetails = await _spotifyAlbumService.GetAlbumDetailsAsync(albumId);
                bool isAvailable = await _vinylService.IsAlbumAvailable(albumId);


                return new AlbumAvailability
                {
                    Album = albumDetails,
                    IsAvailable = isAvailable
                };
            });

            WishlistAlbums = (await Task.WhenAll(tasks)).ToList();
            return Page();
        }
    }
}