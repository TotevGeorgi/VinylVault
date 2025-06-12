using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Vinyl
    {
        public int Id { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string SellerEmail { get; set; }
        public string Status { get; set; } = "Available";
    }
}
