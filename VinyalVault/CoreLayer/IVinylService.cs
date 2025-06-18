using Common;
using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer
{
    public interface IVinylService
    {
        Task<bool> UploadVinyl(Vinyl vinyl);
        Task<List<Vinyl>> GetVinylsBySeller(string sellerEmail);
        Task<bool> DeleteVinyl(int id);
        Task<bool> UpdateVinyl(Vinyl vinyl);
        Task<List<Vinyl>> GetAvailableVinylsByAlbum(string albumId);
        Task<bool> MarkAsSold(int vinylId);
        Task<SpotifyAlbumDetails> GetAlbumDetailsByIdAsync(string albumId);
        Task<bool> IsAlbumAvailable(string albumId);


    }
}
