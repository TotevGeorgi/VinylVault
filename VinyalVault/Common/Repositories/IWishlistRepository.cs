using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IWishlistRepository
    {
        Task<bool> ExistsByAlbum(string userId, string albumId);
        Task<bool> AddByAlbum(string userId, string albumId);

        Task<List<string>> GetAlbumIdsInWishlist(string userId);
    }

}
