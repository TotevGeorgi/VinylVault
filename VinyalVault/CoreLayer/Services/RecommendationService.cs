using Common.DTOs;
using Common.Repositories;
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

        public RecommendationService(IWishlistRepository wishlistRepo, IOrderRepository orderRepo, ISpotifyAlbumService spotify)
        {
            _wishlistRepo = wishlistRepo;
            _orderRepo = orderRepo;
            _spotify = spotify;
        }

        public async Task<List<SpotifyAlbumPreview>> GetRecommendationsAsync(string userId)
        {
            var wishlistAlbumIds = await _wishlistRepo.GetAlbumIdsInWishlist(userId);
            var orders = await _orderRepo.GetOrdersByUser(userId);

            var wishlistAlbums = await _spotify.GetAlbumsByIdsAsync(wishlistAlbumIds);
            var wishlistGenres = wishlistAlbums
                .Where(a => a.Genres != null)
                .SelectMany(a => a.Genres!)
                .ToList();

            var orderedArtists = orders
                .Select(o => o.Artist)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct()
                .ToList();

            var genreCounts = wishlistGenres
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var artistCounts = orderedArtists
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var recommendations = await _spotify.GetRecommendationsByGenresAndArtistsAsync(genreCounts, artistCounts);

            var knownIds = new HashSet<string>(wishlistAlbumIds.Concat(orders.Select(o => o.VinylId.ToString())));
            return recommendations.Where(r => !knownIds.Contains(r.Id)).ToList();
        }
    }
}
