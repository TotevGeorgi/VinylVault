using Common.DTOs;
using Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepo;

        public RatingService(IRatingRepository ratingRepo)
        {
            _ratingRepo = ratingRepo;
        }

        public async Task<bool> AddRatingAsync(SellerRatingDTO rating)
        {
            return await _ratingRepo.AddRatingAsync(rating);
        }

        public async Task<List<SellerRatingDTO>> GetRatingsForSellerAsync(string sellerEmail)
        {
            return await _ratingRepo.GetRatingsForSellerAsync(sellerEmail);
        }
        public async Task<double> GetAverageRatingAsync(string sellerEmail)
        {
            var ratings = await _ratingRepo.GetRatingsForSellerAsync(sellerEmail);
            if (ratings == null || !ratings.Any()) return 0;
            return ratings.Average(r => r.Rating);
        }
    }
}
