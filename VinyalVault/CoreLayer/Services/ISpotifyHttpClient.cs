using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface ISpotifyHttpClient
    {
        Task<string> PostFormAsync(string url, Dictionary<string, string> form, Dictionary<string, string>? headers = null);
        Task<string> GetStringAsync(string url, Dictionary<string, string>? headers = null);
    }
}
