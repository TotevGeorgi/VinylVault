using Common.Repositories;
using CoreLayer.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class WishlistServiceTests
    {
        private readonly WishlistService _svc;
        private readonly Mock<IWishlistRepository> _repo = new();

        public WishlistServiceTests()
        {
            _svc = new WishlistService(_repo.Object);
        }

        [Fact]
        public async Task AddSpotifyAlbumToWishlist_AlreadyExists_ReturnsFalse()
        {
            _repo.Setup(r => r.ExistsByAlbum("u", "a")).ReturnsAsync(true);

            var ok = await _svc.AddSpotifyAlbumToWishlist("u", "a");
            Assert.False(ok);
            _repo.Verify(r => r.AddByAlbum(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddSpotifyAlbumToWishlist_NewAlbum_CallsAddAndReturnsTrue()
        {
            _repo.Setup(r => r.ExistsByAlbum("u", "a")).ReturnsAsync(false);
            _repo.Setup(r => r.AddByAlbum("u", "a")).ReturnsAsync(true);

            var ok = await _svc.AddSpotifyAlbumToWishlist("u", "a");
            Assert.True(ok);
            _repo.Verify(r => r.AddByAlbum("u", "a"), Times.Once);
        }

        [Fact]
        public async Task GetAlbumIdsInWishlist_DelegatesToRepo()
        {
            var list = new List<string> { "x", "y" };
            _repo.Setup(r => r.GetAlbumIdsInWishlist("u")).ReturnsAsync(list);

            var outList = await _svc.GetAlbumIdsInWishlist("u");
            Assert.Same(list, outList);
        }
    }
}
