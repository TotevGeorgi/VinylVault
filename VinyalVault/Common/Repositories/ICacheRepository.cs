using Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface ICacheRepository
    {
        Task<List<PopularRelease>> GetCachedReleasesAsync(string albumType, int pageNumber, int pageSize);
        Task SaveReleasesAsync(List<PopularRelease> releases);
        Task<bool> IsCacheExpiredAsync(string albumType);

        Task<List<PopularRelease>> GetCachedSearchResultsAsync(string query, int pageNumber, int pageSize);
        Task SaveSearchResultsAsync(string query, List<PopularRelease> results);
        Task<bool> IsSearchCacheExpiredAsync(string query);
    }

}
