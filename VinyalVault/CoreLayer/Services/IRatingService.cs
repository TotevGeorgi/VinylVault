using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IRatingService
    {
        Task<bool> AddRatingAsync(SellerRatingDTO rating);
        Task<List<SellerRatingDTO>> GetRatingsForSellerAsync(string sellerEmail);
        Task<double> GetAverageRatingAsync(string sellerEmail);

    }
}
