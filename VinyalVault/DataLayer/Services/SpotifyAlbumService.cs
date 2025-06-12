using Common.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DataLayer.Services
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

        public async Task<List<SpotifyAlbumPreview>> SearchAlbumsAsync(string query)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=album&limit=12");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);

            var albums = new List<SpotifyAlbumPreview>();

            foreach (var item in doc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray())
            {
                var albumId = item.GetProperty("id").GetString();
                var album = new SpotifyAlbumPreview
                {
                    Id = albumId,
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                    CoverUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                };

                var trackResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/albums/{albumId}/tracks?limit=50");
                if (!trackResponse.IsSuccessStatusCode) continue;

                var trackJson = await trackResponse.Content.ReadAsStringAsync();
                var trackDoc = JsonDocument.Parse(trackJson);
                var trackIds = new List<string>();

                foreach (var track in trackDoc.RootElement.GetProperty("items").EnumerateArray())
                {
                    if (track.TryGetProperty("id", out var idNode))
                    {
                        trackIds.Add(idNode.GetString());
                    }
                }

                if (trackIds.Count > 0)
                {
                    var popularitySum = 0;
                    var validTracks = 0;

                    for (int i = 0; i < trackIds.Count; i += 50)
                    {
                        var chunk = trackIds.Skip(i).Take(50);
                        var idsParam = string.Join(",", chunk);

                        var popularityResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/tracks?ids={idsParam}");
                        if (!popularityResponse.IsSuccessStatusCode) continue;

                        var popularityJson = await popularityResponse.Content.ReadAsStringAsync();
                        var popularityDoc = JsonDocument.Parse(popularityJson);

                        foreach (var track in popularityDoc.RootElement.GetProperty("tracks").EnumerateArray())
                        {
                            if (track.TryGetProperty("popularity", out var popularityNode))
                            {
                                popularitySum += popularityNode.GetInt32();
                                validTracks++;
                            }
                        }
                    }

                    if (validTracks > 0)
                    {
                        album.EstimatedPopularity = popularitySum / validTracks;
                    }
                }

                albums.Add(album);
            }

            return albums.OrderByDescending(a => a.EstimatedPopularity ?? 0).ToList();
        }

        public async Task<List<SpotifyAlbumPreview>> SearchAlbumsByTrackAsync(string trackQuery)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(trackQuery)}&type=track&limit=15");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);

            var albumIds = new HashSet<string>();
            var albums = new List<SpotifyAlbumPreview>();

            foreach (var track in doc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray())
            {
                var album = track.GetProperty("album");
                var albumId = album.GetProperty("id").GetString();

                if (!albumIds.Contains(albumId))
                {
                    albumIds.Add(albumId);

                    albums.Add(new SpotifyAlbumPreview
                    {
                        Id = albumId,
                        Name = album.GetProperty("name").GetString(),
                        Artist = album.GetProperty("artists")[0].GetProperty("name").GetString(),
                        CoverUrl = album.GetProperty("images")[0].GetProperty("url").GetString()
                    });
                }
            }

            return albums;
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
                var albums = await SearchAlbumsAsync(genre);
                results.AddRange(albums);
            }

            var distinctAlbums = results
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderByDescending(a => a.EstimatedPopularity ?? 0)
                .ToList();

            return distinctAlbums;
        }

    }
}