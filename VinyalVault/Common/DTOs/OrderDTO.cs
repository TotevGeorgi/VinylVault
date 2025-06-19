using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public int VinylId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public decimal Price { get; set; }
        public string CoverUrl { get; set; }
    }

}
