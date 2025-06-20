using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface ISpotifySettings
    {
        string ClientId { get; }
        string ClientSecret { get; }
    }
}
