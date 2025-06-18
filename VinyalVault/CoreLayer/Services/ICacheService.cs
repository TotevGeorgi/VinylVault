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
        Task<List<SpotifyAlbumPreview>> GetCachedOrFreshAsync(string albumType);
        Task<List<SpotifyAlbumPreview>> GetCachedRecommendationsAsync(string userEmail);
        Task InvalidateUserRecommendationsAsync(string userEmail);
    }
}
