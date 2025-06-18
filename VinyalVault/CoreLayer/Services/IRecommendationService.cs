using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IRecommendationService
    {
        Task<List<SpotifyAlbumPreview>> GetRecommendationsAsync(string userEmail);
    }
}
