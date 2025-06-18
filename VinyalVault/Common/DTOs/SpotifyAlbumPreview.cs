using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class SpotifyAlbumPreview
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string CoverUrl { get; set; }
        public bool IsTrackResult { get; set; } = false;
        public int? EstimatedPopularity { get; set; }
        public string? ReleaseDate { get; set; }
        public int Popularity { get; set; }
        public bool IsAvailable { get; set; }
        public List<string> Genres { get; set; } = new();

    }
}
