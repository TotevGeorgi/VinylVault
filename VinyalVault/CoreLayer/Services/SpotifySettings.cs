using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class SpotifySettings : ISpotifySettings
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
    }
}
