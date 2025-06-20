using Xunit;
using Moq;
using Common.DTOs;
using Common.Repositories;
using CoreLayer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTest
{
    public class RecommendationServiceTests
    {
        private readonly RecommendationService _svc;
        private readonly Mock<IWishlistRepository> _wish = new();
        private readonly Mock<IOrderRepository> _order = new();
        private readonly Mock<ISpotifyAlbumService> _spotify = new();

        public RecommendationServiceTests()
        {
            _spotify
              .Setup(s => s.GetAlbumsByIdsAsync(It.IsAny<List<string>>()))
              .ReturnsAsync(new List<SpotifyAlbumPreview>());

            _spotify
              .Setup(s => s.GetRecommendationsByGenresAndArtistsAsync(
                  It.IsAny<List<string>>(), It.IsAny<List<string>>()))
              .ReturnsAsync(new List<SpotifyAlbumPreview>());

            _svc = new RecommendationService(_wish.Object, _order.Object, _spotify.Object);
        }

        [Fact]
        public async Task GetRecommendationsAsync_EmptyWishlistAndOrders_ReturnsEmpty()
        {
            _wish.Setup(w => w.GetAlbumIdsInWishlist("u"))
                 .ReturnsAsync(new List<string>());
            _order.Setup(o => o.GetOrdersByUser("u"))
                  .ReturnsAsync(new List<OrderDTO>());

            var recs = await _svc.GetRecommendationsAsync("u");

            Assert.Empty(recs);
        }

        [Fact]
        public async Task GetRecommendationsAsync_FiltersOutKnownIds()
        {
            _wish.Setup(w => w.GetAlbumIdsInWishlist("u"))
                 .ReturnsAsync(new List<string> { "X" });
            _order.Setup(o => o.GetOrdersByUser("u"))
                  .ReturnsAsync(new List<OrderDTO>
                    { new() { VinylId = 2, Artist = "Art", Title = "T" } });

            _spotify
              .Setup(s => s.GetRecommendationsByGenresAndArtistsAsync(
                  It.IsAny<List<string>>(), It.IsAny<List<string>>()))
              .ReturnsAsync(new List<SpotifyAlbumPreview>
              {
                  new() { Id = "X" },
                  new() { Id = "2" },
                  new() { Id = "Y" }
              });

            var result = await _svc.GetRecommendationsAsync("u");

            Assert.Single(result);
            Assert.Equal("Y", result[0].Id);
        }
    }
}
