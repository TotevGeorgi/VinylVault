using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class SpotifyAlbumDetails : SpotifyAlbumPreview
    {
        public string ReleaseDate { get; set; }
        public List<string> Tracks { get; set; } = new(); 
    }
}
