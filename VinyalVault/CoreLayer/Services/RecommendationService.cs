using Common.DTOs;
using Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ISpotifyAlbumService _spotify;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            IWishlistRepository wishlistRepo,
            IOrderRepository orderRepo,
            ISpotifyAlbumService spotify,
            ILogger<RecommendationService> logger)
        {
            _wishlistRepo = wishlistRepo;
            _orderRepo = orderRepo;
            _spotify = spotify;
            _logger = logger;
        }

        public async Task<List<SpotifyAlbumPreview>> GetRecommendationsAsync(string userId)
        {
            var wishlistIds = await _wishlistRepo.GetAlbumIdsInWishlist(userId);
            var orders = await _orderRepo.GetOrdersByUser(userId);
            var ownedIds = new HashSet<string>(
                wishlistIds.Concat(orders.Select(o => o.VinylId.ToString()))
            );

            var wishlistAlbums = await _spotify.GetAlbumsByIdsAsync(wishlistIds);
            var allGenres = wishlistAlbums
                .Where(a => a.Genres != null)
                .SelectMany(a => a.Genres!)
                .ToList();

            var topGenres = allGenres
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var topArtists = orders
                .Select(o => o.Artist)
                .Where(a => !string.IsNullOrEmpty(a))
                .GroupBy(a => a!)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            List<SpotifyAlbumPreview> recs;
            if (topGenres.Any() || topArtists.Any())
            {
                try
                {
                    recs = await _spotify
                        .GetRecommendationsByGenresAndArtistsAsync(topGenres, topArtists);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                      "Spotify recs failed; falling back to New Releases");
                    recs = await _spotify.GetNewReleasesAsync(10);
                }
            }
            else
            {
                _logger.LogInformation(
                  "No user seeds available, falling back to New Releases");
                recs = await _spotify.GetNewReleasesAsync(10);
            }

            return recs
                .Where(r => !ownedIds.Contains(r.Id))
                .ToList();
        }
    }
}
