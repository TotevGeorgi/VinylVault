using Common.DTOs;
using Common.Repositories;
using CoreLayer.Services;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            ICacheRepository cacheRepository,
            ISpotifyAlbumService spotify,
            IVinylService vinylService,
            ILogger<CacheService> logger)
        {
            _cacheRepository = cacheRepository;
            _spotify = spotify;
            _vinylService = vinylService;
            _logger = logger;
        }

        public async Task<List<SpotifyAlbumPreview>> GetCachedOrFreshAsync(
            string albumType, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogDebug("Getting {AlbumType} albums (Page {PageNumber}, Size {PageSize})",
                    albumType, pageNumber, pageSize);

                var isExpired = await _cacheRepository.IsCacheExpiredAsync(albumType);
                _logger.LogDebug("Cache for {AlbumType} is expired: {IsExpired}", albumType, isExpired);

                if (isExpired)
                {
                    _logger.LogInformation("Refreshing cache for {AlbumType}", albumType);
                    List<SpotifyAlbumPreview> freshData = albumType switch
                    {
                        "popular" => await _spotify.GetMostPopularAlbumsAsync(pageSize),
                        "new" => await _spotify.GetNewReleasesAsync(pageSize),
                        _ => new List<SpotifyAlbumPreview>()
                    };

                    _logger.LogDebug("Retrieved {Count} fresh {AlbumType} albums", freshData.Count, albumType);

                    var toSave = new List<PopularRelease>();
                    foreach (var album in freshData)
                    {
                        var isAvailable = await _vinylService.IsAlbumAvailable(album.Id);
                        toSave.Add(new PopularRelease
                        {
                            AlbumId = album.Id,
                            Name = album.Name,
                            Artist = album.Artist,
                            Cover = album.CoverUrl,
                            ReleaseDate = album.ReleaseDate ?? DateTime.UtcNow,
                            PopularityScore = album.Popularity,
                            AlbumType = albumType,
                            LastUpdated = DateTime.UtcNow,
                            IsAvailable = isAvailable,
                            Genres = album.Genres
                        });
                    }

                    await _cacheRepository.SaveReleasesAsync(toSave);
                    _logger.LogInformation("Saved {Count} {AlbumType} albums to cache", toSave.Count, albumType);
                }

                var cached = await _cacheRepository.GetCachedReleasesAsync(albumType, pageNumber, pageSize);
                _logger.LogDebug("Retrieved {Count} cached {AlbumType} albums", cached.Count, albumType);

                return cached.Select(c => new SpotifyAlbumPreview
                {
                    Id = c.AlbumId,
                    Name = c.Name,
                    Artist = c.Artist,
                    CoverUrl = c.Cover,
                    ReleaseDate = c.ReleaseDate,
                    Popularity = c.PopularityScore,
                    IsAvailable = c.IsAvailable,
                    Genres = c.Genres
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached or fresh data for {AlbumType}", albumType);
                return new List<SpotifyAlbumPreview>();
            }
        }

        public async Task<List<SpotifyAlbumPreview>> GetCachedSearchAsync(
            string query, int pageNumber, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return new List<SpotifyAlbumPreview>();

                _logger.LogDebug("Searching cache for '{Query}' (Page {PageNumber}, Size {PageSize})",
                    query, pageNumber, pageSize);

                var isExpired = await _cacheRepository.IsSearchCacheExpiredAsync(query);
                _logger.LogDebug("Search cache for '{Query}' is expired: {IsExpired}", query, isExpired);

                if (isExpired)
                {
                    _logger.LogInformation("Refreshing search cache for '{Query}'", query);
                    var freshData = await _spotify.SearchAlbumPreviewsAsync(query);
                    _logger.LogDebug("Retrieved {Count} search results for '{Query}'", freshData.Count, query);

                    var toSave = new List<PopularRelease>();
                    foreach (var album in freshData)
                    {
                        var isAvailable = await _vinylService.IsAlbumAvailable(album.Id);
                        toSave.Add(new PopularRelease
                        {
                            AlbumId = album.Id,
                            Name = album.Name,
                            Artist = album.Artist,
                            Cover = album.CoverUrl,
                            ReleaseDate = album.ReleaseDate ?? DateTime.UtcNow,
                            PopularityScore = album.Popularity,
                            AlbumType = "search",
                            LastUpdated = DateTime.UtcNow,
                            IsAvailable = isAvailable,
                            Query = query,
                            Genres = album.Genres
                        });
                    }

                    await _cacheRepository.SaveSearchResultsAsync(query, toSave);
                    _logger.LogInformation("Saved {Count} search results for '{Query}' to cache", toSave.Count, query);
                }

                var cached = await _cacheRepository.GetCachedSearchResultsAsync(query, pageNumber, pageSize);
                _logger.LogDebug("Retrieved {Count} cached search results for '{Query}'", cached.Count, query);

                return cached.Select(c => new SpotifyAlbumPreview
                {
                    Id = c.AlbumId,
                    Name = c.Name,
                    Artist = c.Artist,
                    CoverUrl = c.Cover,
                    ReleaseDate = c.ReleaseDate,
                    Popularity = c.PopularityScore,
                    IsAvailable = c.IsAvailable,
                    Genres = c.Genres
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached search results for '{Query}'", query);
                return new List<SpotifyAlbumPreview>();
            }
        }

        public async Task<List<SpotifyAlbumPreview>> GetCachedRecommendationsAsync(
            Guid userId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogDebug("Getting recommendations for user {UserId}", userId);
                var cached = await _cacheRepository.GetCachedReleasesAsync("recommendations", pageNumber, pageSize);

                if (cached.Any())
                {
                    _logger.LogDebug("Returning {Count} cached recommendations", cached.Count);
                    return cached.Select(c => new SpotifyAlbumPreview
                    {
                        Id = c.AlbumId,
                        Name = c.Name,
                        Artist = c.Artist,
                        CoverUrl = c.Cover,
                        ReleaseDate = c.ReleaseDate,
                        Popularity = c.PopularityScore,
                        IsAvailable = c.IsAvailable,
                        Genres = c.Genres
                    }).ToList();
                }

                _logger.LogDebug("No cached recommendations found");
                return new List<SpotifyAlbumPreview>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached recommendations for user {UserId}", userId);
                return new List<SpotifyAlbumPreview>();
            }
        }

        public async Task InvalidateUserRecommendationsAsync(string userEmail)
        {
            try
            {
                _logger.LogInformation("Invalidating recommendations for {UserEmail}", userEmail);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating recommendations for {UserEmail}", userEmail);
            }
        }
    }
}