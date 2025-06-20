using Common.DTOs;
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
    public class RatingServiceTests
    {
        private readonly RatingService _svc;
        private readonly Mock<IRatingRepository> _repo = new();

        public RatingServiceTests()
        {
            _svc = new RatingService(_repo.Object);
        }

        [Fact]
        public async Task GetAverageRatingAsync_NoRatings_ReturnsZero()
        {
            _repo.Setup(r => r.GetRatingsForSellerAsync("seller"))
                 .ReturnsAsync(new List<SellerRatingDTO>());

            var avg = await _svc.GetAverageRatingAsync("seller");
            Assert.Equal(0, avg);
        }

        [Fact]
        public async Task GetAverageRatingAsync_WithRatings_ReturnsCorrectAverage()
        {
            var ratings = new List<SellerRatingDTO>
            {
                new() { Rating = 3 },
                new() { Rating = 5 },
                new() { Rating = 4 },
            };
            _repo.Setup(r => r.GetRatingsForSellerAsync("x"))
                 .ReturnsAsync(ratings);

            var avg = await _svc.GetAverageRatingAsync("x");
            Assert.Equal(4.0, avg);
        }

        [Fact]
        public async Task AddRatingAsync_DelegatesToRepo()
        {
            var dto = new SellerRatingDTO { RatingId = Guid.NewGuid() };
            _repo.Setup(r => r.AddRatingAsync(dto)).ReturnsAsync(true);

            var ok = await _svc.AddRatingAsync(dto);
            Assert.True(ok);
            _repo.Verify(r => r.AddRatingAsync(dto), Times.Once);
        }
    }
}
