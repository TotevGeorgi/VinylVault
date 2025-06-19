using System.Collections.Generic;
using System.Threading.Tasks;
using Common.DTOs;

namespace CoreLayer.Services
{
    public interface ISpotifyAlbumService
    {
        Task<List<SpotifyAlbumPreview>> SearchAlbumPreviewsAsync(string query, int limit = 12);
        Task<SpotifyAlbumDetails> GetAlbumDetailsAsync(string albumId);
        Task<(SpotifyAlbumPreview TopTrackAlbum, List<SpotifyAlbumPreview> AlbumMatches)> SearchSmartAsync(string query);
        Task<List<SpotifyAlbumPreview>> GetMostPopularAlbumsAsync();
        Task<List<SpotifyAlbumPreview>> GetNewReleasesAsync(); 
        Task<List<SpotifyAlbumPreview>> GetPopularAlbumsByGenresAsync(List<string> genres);
        Task<List<SpotifyAlbumPreview>> GetAlbumsByIdsAsync(List<string> albumIds);
        Task<List<SpotifyAlbumPreview>> GetRecommendationsByGenresAndArtistsAsync(List<string> genres, List<string> artists);
        Task<List<SpotifyAlbumPreview>> GetRecommendedAlbumsAsync(string userEmail, IOrderService orderService, IWishlistService wishlistService);


    }
}
