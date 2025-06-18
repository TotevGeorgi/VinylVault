using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer
{
    public class AlbumAvailability
    {
        public SpotifyAlbumPreview Album { get; set; }
        public bool IsAvailable { get; set; }
    }
}