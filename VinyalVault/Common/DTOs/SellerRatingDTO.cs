using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class SellerRatingDTO
    {
        public Guid RatingId { get; set; }
        public string BuyerEmail { get; set; }
        public string SellerEmail { get; set; }
        public int VinylId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
