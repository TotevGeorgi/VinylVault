using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IVinylRepository
    {
        Task<Vinyl?> GetVinylById(int id);
        Task<bool> SaveVinyl(Vinyl vinyl);
        Task<List<Vinyl>> GetVinylsBySeller(string sellerEmail);
        Task<bool> DeleteVinyl(int id);
        Task<bool> UpdateVinyl(Vinyl vinyl);
        Task<List<Vinyl>> GetVinylsByAlbumIdAndStatus(string albumId, string status);
        Task<bool> UpdateStatus(int id, string status);
    }

}
