using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IPopularReleaseRepository
    {
        Task<List<PopularRelease>> GetCachedReleasesAsync(string type);
        Task SaveReleasesAsync(List<PopularRelease> releases);
        Task<bool> IsCacheExpiredAsync(string type);
    }
}
