using CoreLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CoreLayer.Services
{
    public class SpotifyGenreService : IGenreService
    {
        private readonly ISpotifyHttpClient _http;
        private readonly ISpotifySettings _settings;

        public SpotifyGenreService(ISpotifyHttpClient httpClient, ISpotifySettings settings)
        {
            _http = httpClient;
            _settings = settings;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };
            var authHeader = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            return await _http.PostFormAsync(
                "https://accounts.spotify.com/api/token",
                form,
                new Dictionary<string, string>
                {
                    { "Authorization", $"Basic {authHeader}" }
                }
            );
        }

        public async Task<List<string>> GetGenresAsync()
        {
            var token = await GetAccessTokenAsync();
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token}" }
            };
            var raw = await _http.GetStringAsync(
                "https://api.spotify.com/v1/recommendations/available-genre-seeds",
                headers);

            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement
                      .GetProperty("genres")
                      .EnumerateArray()
                      .Select(g => g.GetString()!)
                      .ToList();
        }
    }
}
