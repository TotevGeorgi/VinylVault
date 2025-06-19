using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VinylVaultWeb.Pages
{
    public class MarketplaceModel : PageModel
    {
        private readonly ICacheService _cacheService;

        public List<SpotifyAlbumPreview> RecommendedAlbums { get; set; } = new();
        public SpotifyAlbumPreview? TopResult { get; set; }
        public List<AlbumAvailability> Albums { get; set; } = new();
        public List<SpotifyAlbumPreview> MostPopularAlbums { get; set; } = new();
        public List<SpotifyAlbumPreview> NewReleases { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "popular";

        [BindProperty(SupportsGet = true)]
        public List<string> Genres { get; set; } = new();

        public List<string> AvailableGenres { get; set; } = new()
        {
            "Pop", "Rock", "Jazz", "Hip-Hop", "Classical", "Electronic"
        };

        private Guid? CurrentUserId => User?.Identity?.IsAuthenticated == true
            ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
            : null;

        public MarketplaceModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Query))
            {
                var results = await _cacheService.GetCachedSearchAsync(Query, PageNumber, PageSize);
                TopResult = results.FirstOrDefault();
                Albums = results.Skip(1)
                            .Select(a => new AlbumAvailability { Album = a, IsAvailable = a.IsAvailable })
                            .ToList();
            }

            MostPopularAlbums = await _cacheService.GetCachedOrFreshAsync("popular", PageNumber, PageSize);
            NewReleases = await _cacheService.GetCachedOrFreshAsync("new", PageNumber, PageSize);

            if (CurrentUserId.HasValue)
                RecommendedAlbums = await _cacheService.GetCachedRecommendationsAsync(
                    CurrentUserId.Value, PageNumber, PageSize);

            return Page();
        }
    }
}

