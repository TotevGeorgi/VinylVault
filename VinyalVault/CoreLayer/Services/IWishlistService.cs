using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IWishlistService
    {
        Task<bool> AddSpotifyAlbumToWishlist(string userId, string albumId);
        Task<List<string>> GetAlbumIdsInWishlist(string userId);

    }
}
