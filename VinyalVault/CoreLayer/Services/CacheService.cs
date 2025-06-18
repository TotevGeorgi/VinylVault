using Common.DTOs;
using CoreLayer.Services;
using DataLayer;

public class CacheService : ICacheService
{
    private readonly DBCashMarket _dbCashMarket;
    private readonly ISpotifyAlbumService _spotify;
    private readonly Dictionary<string, (List<SpotifyAlbumPreview> albums, DateTime updated)> _userRecCache = new();
    private readonly TimeSpan _recExpiry = TimeSpan.FromHours(24);
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;

    public CacheService(DBCashMarket dbCashMarket, ISpotifyAlbumService spotify, IOrderService orderService, IWishlistService wishlistService)
    {
        _dbCashMarket = dbCashMarket;
        _spotify = spotify;
        _orderService = orderService;
        _wishlistService = wishlistService;
    }

    public async Task<List<SpotifyAlbumPreview>> GetCachedOrFreshAsync(string albumType)
    {
        if (await _dbCashMarket.IsCacheExpiredAsync(albumType))
        {
            List<SpotifyAlbumPreview> freshData = albumType switch
            {
                "popular" => await _spotify.GetMostPopularAlbumsAsync(),
                "new" => await _spotify.GetNewReleasesAsync(),
                _ => new List<SpotifyAlbumPreview>()
            };

            var toSave = freshData.Select(a => new PopularRelease
            {
                AlbumId = a.Id,
                Name = a.Name,
                Artist = a.Artist,
                Cover = a.CoverUrl,
                ReleaseDate = a.ReleaseDate ?? DateTime.UtcNow,
                PopularityScore = a.Popularity,
                AlbumType = albumType,
                LastUpdated = DateTime.UtcNow
            }).ToList();

            await _dbCashMarket.SaveReleasesAsync(toSave);
        }

        var cached = await _dbCashMarket.GetCachedReleasesAsync(albumType);

        return cached.Select(c => new SpotifyAlbumPreview
        {
            Id = c.AlbumId,
            Name = c.Name,
            Artist = c.Artist,
            CoverUrl = c.Cover,
            ReleaseDate = c.ReleaseDate, 
            Popularity = c.PopularityScore
        }).ToList();
    }
    public async Task<List<SpotifyAlbumPreview>> GetCachedRecommendationsAsync(string userEmail)
    {
        if (_userRecCache.TryGetValue(userEmail, out var entry))
        {
            if ((DateTime.UtcNow - entry.updated) < _recExpiry)
            {
                return entry.albums;
            }
        }

        var fresh = await _spotify.GetRecommendedAlbumsAsync(userEmail, _orderService, _wishlistService);

        _userRecCache[userEmail] = (fresh, DateTime.UtcNow);
        return fresh;
    }

    public Task InvalidateUserRecommendationsAsync(string userEmail)
    {
        _userRecCache.Remove(userEmail);
        return Task.CompletedTask;
    }
}
