using Common.DTOs;
using Common.Repositories;
using Common;
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
    public class VinylServiceTests
    {
        private readonly VinylService _svc;
        private readonly Mock<IVinylRepository> _vinylRepo = new();
        private readonly Mock<ISpotifyAlbumService> _spotify = new();

        public VinylServiceTests()
        {
            _svc = new VinylService(_vinylRepo.Object, _spotify.Object);
        }

        [Fact]
        public async Task AddAvailabilityToAlbumsAsync_MapsCorrectly()
        {
            var previews = new List<SpotifyAlbumPreview>
            {
                new() { Id = "A" },
                new() { Id = "B" },
            };

            _vinylRepo.Setup(r => r.GetVinylsByAlbumIdAndStatus("A", "Available"))
                      .ReturnsAsync(new List<Vinyl> { new() { Id = 1 } });
            _vinylRepo.Setup(r => r.GetVinylsByAlbumIdAndStatus("B", "Available"))
                      .ReturnsAsync(new List<Vinyl>());

            var result = await _svc.AddAvailabilityToAlbumsAsync(previews);

            Assert.Collection(result,
                item => { Assert.Equal("A", item.Album.Id); Assert.True(item.IsAvailable); },
                item => { Assert.Equal("B", item.Album.Id); Assert.False(item.IsAvailable); }
            );
        }
    }
}
