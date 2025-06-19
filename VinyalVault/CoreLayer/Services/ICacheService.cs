using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface ICacheService
    {
        Task<List<SpotifyAlbumPreview>> GetCachedOrFreshAsync(string albumType, int pageNumber, int pageSize);
        Task<List<SpotifyAlbumPreview>> GetCachedSearchAsync(string query, int pageNumber, int pageSize);
        Task<List<SpotifyAlbumPreview>> GetCachedRecommendationsAsync(Guid userId, int pageNumber, int pageSize);
        Task InvalidateUserRecommendationsAsync(string userEmail);
    }

}
