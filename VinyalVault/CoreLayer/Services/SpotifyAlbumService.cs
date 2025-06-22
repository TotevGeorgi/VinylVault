using Common.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class SpotifyAlbumService : ISpotifyAlbumService
    {
        private readonly ISpotifyHttpClient _http;
        private readonly ISpotifySettings _settings;
        private readonly ILogger<SpotifyAlbumService> _logger;

        public SpotifyAlbumService(
            ISpotifyHttpClient httpClient,
            ISpotifySettings settings,
            ILogger<SpotifyAlbumService> logger)
        {
            _http = httpClient;
            _settings = settings;
            _logger = logger;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            };
            var authHeader = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")
            );

            var rawJson = await _http.PostFormAsync(
                "https://accounts.spotify.com/api/token",
                form,
                new Dictionary<string, string>
                {
                    ["Authorization"] = $"Basic {authHeader}"
                }
            );

            using var doc = JsonDocument.Parse(rawJson);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        public async Task<List<SpotifyAlbumPreview>> SearchAlbumPreviewsAsync(string query, int limit = 12)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new();

            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            };
            var url = $"https://api.spotify.com/v1/search" +
                      $"?q={Uri.EscapeDataString(query)}" +
                      $"&type=album&limit={limit}";

            var raw = await _http.GetStringAsync(url, headers);
            using var doc = JsonDocument.Parse(raw);

            return doc.RootElement
                      .GetProperty("albums")
                      .GetProperty("items")
                      .EnumerateArray()
                      .Select(it => new SpotifyAlbumPreview
                      {
                          Id = it.GetProperty("id").GetString()!,
                          Name = it.GetProperty("name").GetString()!,
                          Artist = it.GetProperty("artists")[0]
                                             .GetProperty("name")
                                             .GetString()!,
                          CoverUrl = it.GetProperty("images")[0]
                                             .GetProperty("url")
                                             .GetString()!
                      })
                      .ToList();
        }

        public async Task<(SpotifyAlbumPreview TopTrackAlbum, List<SpotifyAlbumPreview> AlbumMatches)> SearchSmartAsync(string query)
        {
            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };

            var trackRaw = await _http.GetStringAsync(
                $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=10",
                headers);
            using var trackDoc = JsonDocument.Parse(trackRaw);

            SpotifyAlbumPreview topTrackAlbum = null!;
            int topPop = -1;
            foreach (var tr in trackDoc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray())
            {
                var pop = tr.GetProperty("popularity").GetInt32();
                if (pop > topPop)
                {
                    var alb = tr.GetProperty("album");
                    topTrackAlbum = new SpotifyAlbumPreview
                    {
                        Id = alb.GetProperty("id").GetString()!,
                        Name = alb.GetProperty("name").GetString()!,
                        Artist = alb.GetProperty("artists")[0].GetProperty("name").GetString()!,
                        CoverUrl = alb.GetProperty("images")[0].GetProperty("url").GetString()!,
                        IsTrackResult = true
                    };
                    topPop = pop;
                }
            }

            var albumRaw = await _http.GetStringAsync(
                $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=album&limit=10",
                headers);
            using var albumDoc = JsonDocument.Parse(albumRaw);

            var matches = new List<SpotifyAlbumPreview>();
            foreach (var it in albumDoc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray())
            {
                var id = it.GetProperty("id").GetString()!;
                if (topTrackAlbum != null && id == topTrackAlbum.Id) continue;

                matches.Add(new SpotifyAlbumPreview
                {
                    Id = id,
                    Name = it.GetProperty("name").GetString()!,
                    Artist = it.GetProperty("artists")[0].GetProperty("name").GetString()!,
                    CoverUrl = it.GetProperty("images")[0].GetProperty("url").GetString()!
                });
            }

            return (topTrackAlbum, matches);
        }

        public async Task<SpotifyAlbumDetails?> GetAlbumDetailsAsync(string albumId)
        {
            if (string.IsNullOrWhiteSpace(albumId))
                return null;

            try
            {
                var token = await GetAccessTokenAsync();
                var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };

                var raw = await _http.GetStringAsync(
                    $"https://api.spotify.com/v1/albums/{albumId}",
                    headers);

                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                return new SpotifyAlbumDetails
                {
                    Id = albumId,
                    Name = root.GetProperty("name").GetString()!,
                    Artist = root.GetProperty("artists")[0].GetProperty("name").GetString()!,
                    CoverUrl = root.GetProperty("images")[0].GetProperty("url").GetString()!,
                    ReleaseDate = root.GetProperty("release_date").GetString()!,
                    Genres = root.TryGetProperty("genres", out var gn)
                                    ? gn.EnumerateArray().Select(g => g.GetString()!).ToList()
                                    : new(),
                    Tracks = root.GetProperty("tracks").GetProperty("items")
                                    .EnumerateArray()
                                    .Select(t => t.GetProperty("name").GetString()!)
                                    .ToList()
                };
            }
            catch
            {
                return null;
            }
        }


        public async Task<List<SpotifyAlbumPreview>> GetNewReleasesAsync(int limit = 20)
        {
            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            };

            var url = $"https://api.spotify.com/v1/browse/new-releases?limit={limit}";
            _logger.LogDebug("Spotify GET {Url}", url);

            var raw = await _http.GetStringAsync(url, headers);
            var snippet = raw.Length > 200 ? raw.Substring(0, 200) + "…" : raw;
            _logger.LogDebug("Raw NewReleases JSON ({Length} chars): {Snippet}", raw.Length, snippet);

            using var doc = JsonDocument.Parse(raw);
            var items = doc.RootElement
                           .GetProperty("albums")
                           .GetProperty("items")
                           .EnumerateArray()
                           .ToList();

            _logger.LogDebug("Parsed NewReleases count: {Count}", items.Count);

            return items
                .Select(it => new SpotifyAlbumPreview
                {
                    Id = it.GetProperty("id").GetString()!,
                    Name = it.GetProperty("name").GetString()!,
                    Artist = it.GetProperty("artists")[0].GetProperty("name").GetString()!,
                    CoverUrl = it.GetProperty("images")[0].GetProperty("url").GetString()!
                })
                .ToList();
        }

        public Task<List<SpotifyAlbumPreview>> GetMostPopularAlbumsAsync(int limit = 20)
            => GetNewReleasesAsync(limit);

        public async Task<List<SpotifyAlbumPreview>> GetPopularAlbumsByGenresAsync(List<string> genres)
        {
            var results = new List<SpotifyAlbumPreview>();
            foreach (var g in genres)
                results.AddRange(await SearchAlbumPreviewsAsync(g));
            return results
                   .GroupBy(a => a.Id)
                   .Select(g => g.First())
                   .ToList();
        }

        public async Task<List<SpotifyAlbumPreview>> GetAlbumsByIdsAsync(List<string> albumIds)
        {
            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };
            var list = new List<SpotifyAlbumPreview>();

            foreach (var id in albumIds)
            {
                var raw = await _http.GetStringAsync(
                    $"https://api.spotify.com/v1/albums/{id}",
                    headers);

                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                list.Add(new SpotifyAlbumPreview
                {
                    Id = id,
                    Name = root.GetProperty("name").GetString()!,
                    Artist = root.GetProperty("artists")[0].GetProperty("name").GetString()!,
                    CoverUrl = root.GetProperty("images")[0].GetProperty("url").GetString()!,
                    Genres = root.TryGetProperty("genres", out var gn)
                                   ? gn.EnumerateArray().Select(g2 => g2.GetString()!).ToList()
                                   : new()
                });
            }

            return list;
        }

        public async Task<List<SpotifyAlbumPreview>> GetRecommendationsByGenresAndArtistsAsync(
            List<string> genres, List<string> artists)
        {
            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };

            var genreSeed = string.Join(",", genres.Take(2));
            var seeds = new List<string>();
            if (!string.IsNullOrEmpty(genreSeed))
                seeds.Add($"seed_genres={Uri.EscapeDataString(genreSeed)}");

            var artistIds = new List<string>();
            foreach (var name in artists.Take(3))
            {
                var artistRaw = await _http.GetStringAsync(
                    $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(name)}&type=artist&limit=1",
                    headers);

                using var doc = JsonDocument.Parse(artistRaw);
                var items = doc.RootElement.GetProperty("artists").GetProperty("items");
                if (items.GetArrayLength() > 0)
                {
                    var id = items[0].GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(id)) artistIds.Add(id);
                }
            }
            if (artistIds.Any())
                seeds.Add($"seed_artists={Uri.EscapeDataString(string.Join(",", artistIds))}");

            seeds.Add("limit=15");
            var qs = string.Join("&", seeds);

            try
            {
                var json = await _http.GetStringAsync(
                    $"https://api.spotify.com/v1/recommendations?{qs}",
                    headers);

                using var doc = JsonDocument.Parse(json);
                var seen = new HashSet<string>();
                var outList = new List<SpotifyAlbumPreview>();

                foreach (var t in doc.RootElement.GetProperty("tracks").EnumerateArray())
                {
                    var alb = t.GetProperty("album");
                    var id = alb.GetProperty("id").GetString()!;
                    if (seen.Add(id))
                    {
                        outList.Add(new SpotifyAlbumPreview
                        {
                            Id = id,
                            Name = alb.GetProperty("name").GetString()!,
                            Artist = alb.GetProperty("artists")[0].GetProperty("name").GetString()!,
                            CoverUrl = alb.GetProperty("images")[0].GetProperty("url").GetString()!
                        });
                    }
                }

                return outList;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[WARN] Spotify recommendations failed: {ex.Message}");
                return new();
            }
        }

        public async Task<List<SpotifyAlbumPreview>> GetRecommendedAlbumsAsync(
            string userEmail,
            IOrderService orderService,
            IWishlistService wishlistService)
        {
            var orders = await orderService.GetOrdersByUser(userEmail);
            var wishes = await wishlistService.GetAlbumIdsInWishlist(userEmail);

            var keywords = new HashSet<string>(
                orders.Select(o => o.Artist).Where(a => !string.IsNullOrEmpty(a))
                      .Concat(orders.Select(o => o.Title))
                      .Concat(wishes)
            );

            var recs = new List<SpotifyAlbumPreview>();
            foreach (var kw in keywords.Take(10))
                recs.AddRange((await SearchAlbumPreviewsAsync(kw)).Take(3));

            return recs
                   .GroupBy(a => a.Id)
                   .Select(g => g.First())
                   .ToList();
        }
    }
}
