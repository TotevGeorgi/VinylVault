using Common.DTOs;
using Common.Repositories;
using CoreLayer.Services;
using CoreLayer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class CacheServiceTests
    {
        private readonly CacheService _svc;
        private readonly Mock<ICacheRepository> _cacheRepo = new();
        private readonly Mock<ISpotifyAlbumService> _spotify = new();
        private readonly Mock<IVinylService> _vinyl = new();

        public CacheServiceTests()
        {
            _svc = new CacheService(_cacheRepo.Object, _spotify.Object, _vinyl.Object);
        }

        [Fact]
        public async Task GetCachedOrFreshAsync_WhenCacheExpired_SavesAndReturnsMapped()
        {
            _cacheRepo.Setup(c => c.IsCacheExpiredAsync("popular"))
                      .ReturnsAsync(true);
            _cacheRepo.Setup(c => c.SaveReleasesAsync(It.IsAny<List<PopularRelease>>()))
                      .Returns(Task.CompletedTask);
            _cacheRepo.Setup(c => c.GetCachedReleasesAsync("popular", 1, 10))
                      .ReturnsAsync(new List<PopularRelease>
                        {
                            new()
                            {
                                AlbumId = "A",
                                Name = "N",
                                Artist = "Ar",
                                Cover = "C",
                                ReleaseDate = System.DateTime.UtcNow,
                                PopularityScore = 5,
                                IsAvailable = true
                            }
                        });

            _spotify.Setup(s => s.GetMostPopularAlbumsAsync())
                    .ReturnsAsync(new List<SpotifyAlbumPreview>());
            _vinyl.Setup(v => v.IsAlbumAvailable(It.IsAny<string>()))
                  .ReturnsAsync(true);

            var outList = await _svc.GetCachedOrFreshAsync("popular", 1, 10);
            Assert.Single(outList);
            Assert.Equal("A", outList[0].Id);
        }
    }
}
