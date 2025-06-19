using Common.DTOs;
using Common.Repositories;
using CoreLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly ISpotifyAlbumService _spotify;
        private readonly IVinylService _vinylService;

        public CacheService(
            ICacheRepository cacheRepository,
            ISpotifyAlbumService spotify,
            IVinylService vinylService)
        {
            _cacheRepository = cacheRepository;
            _spotify = spotify;
            _vinylService = vinylService;
        }

        public async Task<List<SpotifyAlbumPreview>> GetCachedOrFreshAsync(
            string albumType, int pageNumber, int pageSize)
        {
            if (await _cacheRepository.IsCacheExpiredAsync(albumType))
            {
                var freshData = albumType switch
                {
                    "popular" => await _spotify.GetMostPopularAlbumsAsync(),
                    "new" => await _spotify.GetNewReleasesAsync(),
                    _ => new List<SpotifyAlbumPreview>()
                };

                var toSave = new List<PopularRelease>();
                foreach (var a in freshData)
                {
                    var isAvailable = await _vinylService.IsAlbumAvailable(a.Id);
                    toSave.Add(new PopularRelease
                    {
                        AlbumId = a.Id,
                        Name = a.Name,
                        Artist = a.Artist,
                        Cover = a.CoverUrl,
                        ReleaseDate = a.ReleaseDate ?? DateTime.UtcNow,
                        PopularityScore = a.Popularity,
                        AlbumType = albumType,
                        LastUpdated = DateTime.UtcNow,
                        IsAvailable = isAvailable
                    });
                }

                await _cacheRepository.SaveReleasesAsync(toSave);
            }

            var cached = await _cacheRepository.GetCachedReleasesAsync(
                albumType, pageNumber, pageSize);

            return cached.Select(c => new SpotifyAlbumPreview
            {
                Id = c.AlbumId,
                Name = c.Name,
                Artist = c.Artist,
                CoverUrl = c.Cover,
                ReleaseDate = c.ReleaseDate,
                Popularity = c.PopularityScore,
                IsAvailable = c.IsAvailable
            }).ToList();
        }

        public async Task<List<SpotifyAlbumPreview>> GetCachedSearchAsync(
            string query, int pageNumber, int pageSize)
        {
            if (await _cacheRepository.IsSearchCacheExpiredAsync(query))
            {
                var freshData = await _spotify.SearchAlbumPreviewsAsync(query);

                var toSave = new List<PopularRelease>();
                foreach (var a in freshData)
                {
                    var isAvailable = await _vinylService.IsAlbumAvailable(a.Id);
                    toSave.Add(new PopularRelease
                    {
                        AlbumId = a.Id,
                        Name = a.Name,
                        Artist = a.Artist,
                        Cover = a.CoverUrl,
                        ReleaseDate = a.ReleaseDate ?? DateTime.UtcNow,
                        PopularityScore = a.Popularity,
                        AlbumType = "search",
                        LastUpdated = DateTime.UtcNow,
                        IsAvailable = isAvailable,
                        Query = query
                    });
                }

                await _cacheRepository.SaveSearchResultsAsync(query, toSave);
            }

            var cached = await _cacheRepository.GetCachedSearchResultsAsync(
                query, pageNumber, pageSize);

            return cached.Select(c => new SpotifyAlbumPreview
            {
                Id = c.AlbumId,
                Name = c.Name,
                Artist = c.Artist,
                CoverUrl = c.Cover,
                ReleaseDate = c.ReleaseDate,
                Popularity = c.PopularityScore,
                IsAvailable = c.IsAvailable
            }).ToList();
        }

        public Task<List<SpotifyAlbumPreview>> GetCachedRecommendationsAsync(
            Guid userId, int pageNumber, int pageSize)
        {
            return Task.FromResult(new List<SpotifyAlbumPreview>());
        }

        public Task InvalidateUserRecommendationsAsync(string userEmail)
            => Task.CompletedTask;
    }
}
