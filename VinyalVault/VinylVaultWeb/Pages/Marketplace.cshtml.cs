using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.DTOs;
using CoreLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CoreLayer;

namespace VinylVaultWeb.Pages
{
    public class MarketplaceModel : PageModel
    {
        private readonly ICacheService _cacheService;
        private readonly IRecommendationService _recService;
        private readonly ISpotifyAlbumService _spotifyAlbumService;
        private readonly ILogger<MarketplaceModel> _logger;

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
            "Pop", "Rock", "Jazz", "Hip-Hop", "Classical", "Electronic"
        };

        [BindProperty(SupportsGet = true)]
        public List<string> Genres { get; set; } = new();

        private Guid? CurrentUserId =>
            User?.Identity?.IsAuthenticated == true
                ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                : null;

        public MarketplaceModel(
            ICacheService cacheService,
            IRecommendationService recService,
            ISpotifyAlbumService spotifyAlbumService,
            ILogger<MarketplaceModel> logger)
        {
            _cacheService = cacheService;
            _recService = recService;
            _spotifyAlbumService = spotifyAlbumService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                MostPopularAlbums = await _cacheService.GetCachedOrFreshAsync("popular", PageNumber, PageSize);
                NewReleases = await _cacheService.GetCachedOrFreshAsync("new", PageNumber, PageSize);
                _logger.LogInformation(
                    "Retrieved {PopularCount} popular and {NewCount} new albums",
                    MostPopularAlbums.Count, NewReleases.Count);

                if (CurrentUserId.HasValue)
                {
                    RecommendedAlbums = await _recService.GetRecommendationsAsync(CurrentUserId.Value.ToString());
                    _logger.LogInformation("Retrieved {Count} recommended albums", RecommendedAlbums.Count);
                }

                if (!string.IsNullOrWhiteSpace(Query))
                {
                    var results = await _cacheService.GetCachedSearchAsync(Query, PageNumber, PageSize);
                    _logger.LogInformation("Found {Count} search results for query: {Query}", results.Count, Query);

                    TopResult = results.FirstOrDefault();
                    Albums = results.Skip(1)
                                       .Select(a => new AlbumAvailability { Album = a, IsAvailable = a.IsAvailable })
                                       .ToList();
                }

                if (string.IsNullOrWhiteSpace(Query) && Genres.Any())
                {
                    MostPopularAlbums = MostPopularAlbums
                        .Where(a => a.Genres != null
                                 && Genres.Intersect(a.Genres, StringComparer.OrdinalIgnoreCase).Any())
                        .ToList();

                    NewReleases = NewReleases
                        .Where(a => a.Genres != null
                                 && Genres.Intersect(a.Genres, StringComparer.OrdinalIgnoreCase).Any())
                        .ToList();

                    _logger.LogInformation(
                        "After genre filtering: {PopularCount} popular and {NewCount} new albums",
                        MostPopularAlbums.Count, NewReleases.Count);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading marketplace data");
                RecommendedAlbums = new();
                MostPopularAlbums = new();
                NewReleases = new();
                TopResult = null;
                Albums = new();
                return Page();
            }
        }
    }
}