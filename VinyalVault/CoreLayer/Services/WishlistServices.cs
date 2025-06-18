using Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepo;

        public WishlistService(IWishlistRepository wishlistRepo)
        {
            _wishlistRepo = wishlistRepo;
        }

        public async Task<bool> AddSpotifyAlbumToWishlist(string userId, string albumId)
        {
            if (await _wishlistRepo.ExistsByAlbum(userId, albumId))
                return false;

            return await _wishlistRepo.AddByAlbum(userId, albumId);
        }
        public async Task<List<string>> GetAlbumIdsInWishlist(string userId)
        {
            return await _wishlistRepo.GetAlbumIdsInWishlist(userId);
        }

    }

}
