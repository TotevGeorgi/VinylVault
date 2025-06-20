using System.Collections.Generic;
using System.Threading.Tasks;
using Common.DTOs;

namespace CoreLayer.Services
{
    public interface ISpotifyAlbumService
    {
        Task<List<SpotifyAlbumPreview>> SearchAlbumPreviewsAsync(string query, int limit = 12);
        Task<(SpotifyAlbumPreview TopTrackAlbum, List<SpotifyAlbumPreview> AlbumMatches)> SearchSmartAsync(string query);
        Task<SpotifyAlbumDetails?> GetAlbumDetailsAsync(string albumId);
        Task<List<SpotifyAlbumPreview>> GetMostPopularAlbumsAsync(int limit = 20);
        Task<List<SpotifyAlbumPreview>> GetNewReleasesAsync(int limit = 20);
        Task<List<SpotifyAlbumPreview>> GetPopularAlbumsByGenresAsync(List<string> genres);
        Task<List<SpotifyAlbumPreview>> GetAlbumsByIdsAsync(List<string> albumIds);
        Task<List<SpotifyAlbumPreview>> GetRecommendationsByGenresAndArtistsAsync(List<string> genres, List<string> artists);
        Task<List<SpotifyAlbumPreview>> GetRecommendedAlbumsAsync(string userEmail, IOrderService orderService, IWishlistService wishlistService);
    }
}
