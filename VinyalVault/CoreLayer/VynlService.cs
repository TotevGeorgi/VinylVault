using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using DataLayer;
using Common.DTOs;
using System.Threading.Tasks;
using CoreLayer.Services;
using Common.Repositories;

namespace CoreLayer
{
    public class VinylService : IVinylService
    {
        private readonly ISpotifyAlbumService _spotifyAlbumService;
        private readonly IVinylRepository _dbVinyl;

        public VinylService(IVinylRepository dbVinyl, ISpotifyAlbumService spotifyAlbumService)
        {
            _dbVinyl = dbVinyl;
            _spotifyAlbumService = spotifyAlbumService;
        }

        public async Task<bool> UploadVinyl(Vinyl vinyl)
        {
            return await _dbVinyl.SaveVinyl(vinyl);
        }

        public async Task<List<Vinyl>> GetVinylsBySeller(string sellerEmail)
        {
            return await _dbVinyl.GetVinylsBySeller(sellerEmail); 
        }

        public async Task<bool> DeleteVinyl(int id)
        {
            return await _dbVinyl.DeleteVinyl(id);
        }

        public async Task<bool> UpdateVinyl(Vinyl vinyl)
        {
            return await _dbVinyl.UpdateVinyl(vinyl);
        }
        public async Task<List<Vinyl>> GetAvailableVinylsByAlbum(string albumId)
        {
            return await _dbVinyl.GetVinylsByAlbumIdAndStatus(albumId, "Available");
        }

        public async Task<bool> MarkAsSold(int vinylId)
        {
            return await _dbVinyl.UpdateStatus(vinylId, "Sold");
        }
        public async Task<Vinyl?> GetVinylById(int id)
        {
            return await _dbVinyl.GetVinylById(id);
        }

        public async Task<SpotifyAlbumDetails> GetAlbumDetailsByIdAsync(string albumId)
        {
            return await _spotifyAlbumService.GetAlbumDetailsAsync(albumId);
        }
        public async Task<bool> IsAlbumAvailable(string albumId)
        {
            var vinyls = await _dbVinyl.GetVinylsByAlbumIdAndStatus(albumId, "Available");
            return vinyls.Any();
        }
        public async Task<List<AlbumAvailability>> AddAvailabilityToAlbumsAsync(List<SpotifyAlbumPreview> albums)
        {
            var tasks = albums.Select(async album =>
            {
                bool available = await IsAlbumAvailable(album.Id);
                return new AlbumAvailability
                {
                    Album = album,
                    IsAvailable = available
                };
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

    }
}
