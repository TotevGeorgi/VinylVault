using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IRatingRepository
    {
        Task<bool> AddRatingAsync(SellerRatingDTO rating);
        Task<List<SellerRatingDTO>> GetRatingsForSellerAsync(string sellerEmail);
    }
}
