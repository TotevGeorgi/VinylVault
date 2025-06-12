using Common.DTOs;
using CoreLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VinylVaultWeb.Pages
{
    public class MarketplaceModel : PageModel
    {
        private readonly ISpotifyAlbumService _albumService;

        public SpotifyAlbumPreview? TopResult { get; set; }
        public List<SpotifyAlbumPreview> Albums { get; set; } = new();
        public List<SpotifyAlbumPreview> MostPopularAlbums { get; set; } = new();
        public List<SpotifyAlbumPreview> NewReleases { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "popular";

        [BindProperty(SupportsGet = true)]
        public List<string> Genres { get; set; } = new();

        public List<string> AvailableGenres { get; set; } = new();

        public MarketplaceModel(ISpotifyAlbumService albumService)
        {
            _albumService = albumService;
        }

        public async Task OnGetAsync()
        {
            AvailableGenres = new List<string> { "Pop", "Rock", "Jazz", "Hip-Hop", "Electronic", "Funk", "Disco", "Country" };

            if (!string.IsNullOrWhiteSpace(Query))
            {
                (TopResult, Albums) = await _albumService.SearchSmartAsync(Query);

                if (Genres.Any())
                {
                    Albums = Albums.Where(a =>
                        a.Genres != null && a.Genres.Any(g => Genres.Contains(g, System.StringComparer.OrdinalIgnoreCase))
                    ).ToList();
                }

                Albums = SortBy switch
                {
                    "newest" => Albums.OrderByDescending(a => a.ReleaseDate).ToList(),
                    "mostlistened" => Albums.OrderByDescending(a => a.Popularity).ToList(),
                    _ => Albums
                };

                MostPopularAlbums.Clear();
                NewReleases.Clear();
            }
            else
            {
                NewReleases = await _albumService.GetNewReleasesAsync();

                if (Genres.Any())
                {
                    MostPopularAlbums = await _albumService.GetPopularAlbumsByGenresAsync(Genres);
                }
                else
                {
                    MostPopularAlbums = await _albumService.GetMostPopularAlbumsAsync();
                }

                TopResult = null;
                Albums.Clear();
            }
        }
    }
}
