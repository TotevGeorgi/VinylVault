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
        private readonly IRecommendationService _recService;

        public List<SpotifyAlbumPreview> RecommendedAlbums { get; set; } = new();
        public List<SpotifyAlbumPreview> MostPopularAlbums { get; set; } = new();
        public List<SpotifyAlbumPreview> NewReleases { get; set; } = new();
        public SpotifyAlbumPreview? TopResult { get; set; }
        public List<AlbumAvailability> Albums { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "popular";

        public List<string> AvailableGenres { get; } = new()
        {
            "Pop","Rock","Jazz","Hip-Hop","Classical","Electronic"
        };

        [BindProperty(SupportsGet = true)]
        public List<string> Genres { get; set; } = new();

        private Guid? CurrentUserId =>
            User?.Identity?.IsAuthenticated == true
                ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                : null;

        public MarketplaceModel(
            ICacheService cacheService,
            IRecommendationService recService)
        {
            _cacheService = cacheService;
            _recService = recService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Query))
            {
                var results = await _cacheService.GetCachedSearchAsync(
                    Query, PageNumber, PageSize);

                TopResult = results.FirstOrDefault();
                Albums = results.Skip(1)
                                .Select(a => new AlbumAvailability
                                {
                                    Album = a,
                                    IsAvailable = a.IsAvailable
                                })
                                .ToList();
            }

            foreach (var g in Genres)
            {
                if (!AvailableGenres.Contains(g))
                {
                    ModelState.AddModelError(
                        nameof(Genres),
                        $"Unknown genre: {g}"
                    );
                    break;
                }
            }
            if (!ModelState.IsValid)
                return Page();

            MostPopularAlbums = await _cacheService.GetCachedOrFreshAsync(
                "popular", PageNumber, PageSize);
            NewReleases = await _cacheService.GetCachedOrFreshAsync(
                "new", PageNumber, PageSize);

            if (Genres.Any())
            {
                MostPopularAlbums = MostPopularAlbums
                    .Where(a => Genres.Intersect(a.Genres ?? Enumerable.Empty<string>()).Any())
                    .ToList();
                NewReleases = NewReleases
                    .Where(a => Genres.Intersect(a.Genres ?? Enumerable.Empty<string>()).Any())
                    .ToList();
            }

            if (CurrentUserId.HasValue)
            {
                var email = User.FindFirst(ClaimTypes.Email)!.Value;
                RecommendedAlbums = await _recService.GetRecommendationsAsync(email);
            }

            return Page();
        }
    }
}
