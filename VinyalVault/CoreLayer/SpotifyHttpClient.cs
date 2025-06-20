using CoreLayer.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer
{
    public class SpotifyHttpClient : ISpotifyHttpClient
    {
        private readonly HttpClient _inner;
        public SpotifyHttpClient(HttpClient inner) => _inner = inner;

        public async Task<string> PostFormAsync(string url,
                                                Dictionary<string, string> form,
                                                Dictionary<string, string>? headers = null)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(form)
            };
            if (headers != null)
                foreach (var h in headers) req.Headers.TryAddWithoutValidation(h.Key, h.Value);

            using var resp = await _inner.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task<string> GetStringAsync(string url,
                                                 Dictionary<string, string>? headers = null)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
                foreach (var h in headers) req.Headers.TryAddWithoutValidation(h.Key, h.Value);

            using var resp = await _inner.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
    }

}
