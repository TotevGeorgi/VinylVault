using Common.DTOs;
using DataLayer;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CoreLayer.Services
{
    public class SpotifyAlbumService : ISpotifyAlbumService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public SpotifyAlbumService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _clientId = config["Spotify:ClientId"];
            _clientSecret = config["Spotify:ClientSecret"];
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<List<SpotifyAlbumPreview>> SearchAlbumPreviewsAsync(
            string query,
            int limit = 12)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://api.spotify.com/v1/search" +
                      $"?q={Uri.EscapeDataString(query)}" +
                      $"&type=album&limit={limit}";

            using var rsp = await _httpClient.GetAsync(url);
            rsp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(
                await rsp.Content.ReadAsStringAsync()
            );

            var items = doc.RootElement
                           .GetProperty("albums")
                           .GetProperty("items")
                           .EnumerateArray();

            var previews = new List<SpotifyAlbumPreview>();
            foreach (var itm in items)
            {
                previews.Add(new SpotifyAlbumPreview
                {
                    Id = itm.GetProperty("id").GetString()!,
                    Name = itm.GetProperty("name").GetString()!,
                    Artist = itm.GetProperty("artists")[0]
                                     .GetProperty("name")
                                     .GetString()!,
                    CoverUrl = itm.GetProperty("images")[0]
                                     .GetProperty("url")
                                     .GetString()!
                });
            }

            return previews;
        }

        public async Task<(SpotifyAlbumPreview TopTrackAlbum, List<SpotifyAlbumPreview> AlbumMatches)>
        SearchSmartAsync(string query)
            {
                var token = await GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var trackResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=10");
                trackResponse.EnsureSuccessStatusCode();
                var trackContent = await trackResponse.Content.ReadAsStringAsync();
                var trackDoc = JsonDocument.Parse(trackContent);

                SpotifyAlbumPreview topTrackAlbum = null;
                int topPopularity = -1;

                foreach (var track in trackDoc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray())
                {
                    int popularity = track.GetProperty("popularity").GetInt32();

                    if (popularity > topPopularity)
                    {
                        var album = track.GetProperty("album");

                        topTrackAlbum = new SpotifyAlbumPreview
                        {
                            Id = album.GetProperty("id").GetString(),
                            Name = album.GetProperty("name").GetString(),
                            Artist = album.GetProperty("artists")[0].GetProperty("name").GetString(),
                            CoverUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                            IsTrackResult = true
                        };

                        topPopularity = popularity;
                    }
                }

                var albumResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=album&limit=10");
                albumResponse.EnsureSuccessStatusCode();
                var albumContent = await albumResponse.Content.ReadAsStringAsync();
                var albumDoc = JsonDocument.Parse(albumContent);

                var albumMatches = new List<SpotifyAlbumPreview>();

                foreach (var item in albumDoc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray())
                {
                    var albumId = item.GetProperty("id").GetString();

                    if (topTrackAlbum != null && albumId == topTrackAlbum.Id)
                        continue;

                    albumMatches.Add(new SpotifyAlbumPreview
                    {
                        Id = albumId,
                        Name = item.GetProperty("name").GetString(),
                        Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                        CoverUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                    });
                }

                return (topTrackAlbum, albumMatches);
            }

        public async Task<SpotifyAlbumDetails> GetAlbumDetailsAsync(string albumId)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/albums/{albumId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var album = new SpotifyAlbumDetails
            {
                Id = albumId,
                Name = root.GetProperty("name").GetString(),
                Artist = root.GetProperty("artists")[0].GetProperty("name").GetString(),
                CoverUrl = root.GetProperty("images")[0].GetProperty("url").GetString(),
                ReleaseDate = root.GetProperty("release_date").GetString(),
                Genres = root.TryGetProperty("genres", out var genresNode)
                            ? genresNode.EnumerateArray().Select(g => g.GetString()).ToList()
                            : new List<string>(),
                Tracks = root.GetProperty("tracks").GetProperty("items").EnumerateArray()
                             .Select(track => track.GetProperty("name").GetString())
                             .ToList()
            };

            return album;
        }
        public async Task<List<SpotifyAlbumPreview>> GetMostPopularAlbumsAsync()
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/browse/new-releases?limit=20");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);

            var albums = new List<SpotifyAlbumPreview>();

            foreach (var item in doc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray())
            {
                var albumId = item.GetProperty("id").GetString();

                albums.Add(new SpotifyAlbumPreview
                {
                    Id = albumId,
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                    CoverUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                });
            }

            return albums;
        }
        public async Task<List<SpotifyAlbumPreview>> GetNewReleasesAsync()
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/browse/new-releases?limit=20");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);

            var albums = new List<SpotifyAlbumPreview>();

            foreach (var item in doc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray())
            {
                var albumId = item.GetProperty("id").GetString();

                albums.Add(new SpotifyAlbumPreview
                {
                    Id = albumId,
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                    CoverUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                });
            }

            return albums;
        }

        public async Task<List<SpotifyAlbumPreview>> GetPopularAlbumsByGenresAsync(List<string> genres)
        {
            var results = new List<SpotifyAlbumPreview>();

            foreach (var genre in genres)
            {
                var albums = await SearchAlbumPreviewsAsync(genre);
                results.AddRange(albums);
            }

            var distinctAlbums = results
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderByDescending(a => a.EstimatedPopularity ?? 0)
                .ToList();

            return distinctAlbums;
        }
        public async Task<List<SpotifyAlbumPreview>> GetAlbumsByIdsAsync(List<string> albumIds)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var albums = new List<SpotifyAlbumPreview>();

            foreach (var id in albumIds)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/albums/{id}");
                    if (!response.IsSuccessStatusCode) continue;

                    var content = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    var preview = new SpotifyAlbumPreview
                    {
                        Id = id,
                        Name = root.GetProperty("name").GetString(),
                        Artist = root.GetProperty("artists")[0].GetProperty("name").GetString(),
                        CoverUrl = root.GetProperty("images")[0].GetProperty("url").GetString(),
                        Genres = root.TryGetProperty("genres", out var genresNode)
                                    ? genresNode.EnumerateArray().Select(g => g.GetString()).ToList()
                                    : new List<string>()
                    };

                    albums.Add(preview);
                }
                catch
                {
                    continue;
                }
            }

            return albums;
        }
        public async Task<List<SpotifyAlbumPreview>> GetRecommendationsByGenresAndArtistsAsync(List<string> genres, List<string> artists)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var genreSeed = string.Join(",", genres.Take(2));  
            var artistSeedList = new List<string>();

            foreach (var artistName in artists.Take(3))
            {
                var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(artistName)}&type=artist&limit=1");
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                var items = doc.RootElement.GetProperty("artists").GetProperty("items");
                if (items.GetArrayLength() > 0)
                {
                    var artistId = items[0].GetProperty("id").GetString();
                    artistSeedList.Add(artistId);
                }
            }

            var artistSeed = string.Join(",", artistSeedList);
            var seedParam = $"seed_genres={genreSeed}&seed_artists={artistSeed}";

            var recResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/recommendations?{seedParam}&limit=15");
            recResponse.EnsureSuccessStatusCode();

            var recContent = await recResponse.Content.ReadAsStringAsync();
            var recDoc = JsonDocument.Parse(recContent);

            var albums = new List<SpotifyAlbumPreview>();
            var seenAlbumIds = new HashSet<string>();

            foreach (var track in recDoc.RootElement.GetProperty("tracks").EnumerateArray())
            {
                var album = track.GetProperty("album");
                var albumId = album.GetProperty("id").GetString();

                if (seenAlbumIds.Contains(albumId)) continue;
                seenAlbumIds.Add(albumId);

                albums.Add(new SpotifyAlbumPreview
                {
                    Id = albumId,
                    Name = album.GetProperty("name").GetString(),
                    Artist = album.GetProperty("artists")[0].GetProperty("name").GetString(),
                    CoverUrl = album.GetProperty("images")[0].GetProperty("url").GetString()
                });
            }

            return albums;
        }
        public async Task<List<SpotifyAlbumPreview>> GetRecommendedAlbumsAsync(string userEmail, IOrderService orderService, IWishlistService wishlistService)
        {
            var recommended = new List<SpotifyAlbumPreview>();

            var orderHistory = await orderService.GetOrdersByUser(userEmail);
            var wishlistAlbumIds = await wishlistService.GetAlbumIdsInWishlist(userEmail);

            var keywords = new HashSet<string>();

            foreach (var order in orderHistory)
            {
                if (!string.IsNullOrWhiteSpace(order.Artist)) keywords.Add(order.Artist);
                if (!string.IsNullOrWhiteSpace(order.Title)) keywords.Add(order.Title);
            }

            foreach (var albumId in wishlistAlbumIds)
            {
                keywords.Add(albumId);
            }

            foreach (var keyword in keywords.Take(10))
            {
                var results = await SearchAlbumPreviewsAsync(keyword);
                recommended.AddRange(results.Take(3));
            }

            return recommended
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderByDescending(a => a.Popularity)
                .ToList();
        }

    }
}