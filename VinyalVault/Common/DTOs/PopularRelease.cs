using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class PopularRelease
    {
        [Key]
        public string AlbumId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Cover { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int PopularityScore { get; set; }
        public string AlbumType { get; set; } = string.Empty; 
        public DateTime LastUpdated { get; set; }
        public bool IsAvailable { get; set; } = false;

    }
}
