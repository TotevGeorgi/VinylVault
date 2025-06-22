using Common.DTOs;
using Common.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataLayer
{
    public class DbCacheMarket : ICacheRepository
    {
        private readonly DBConnection _connection;
        private readonly ILogger<DbCacheMarket> _logger;

        public DbCacheMarket(DBConnection connection, ILogger<DbCacheMarket> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<List<PopularRelease>> GetCachedReleasesAsync(
            string albumType, int pageNumber, int pageSize)
        {
            var list = new List<PopularRelease>();
            try
            {
                using var conn = _connection.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT * FROM PopularRelease
                    WHERE album_type = @type AND (query IS NULL OR query = '')
                    ORDER BY popularity_score DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

                cmd.Parameters.AddWithValue("@type", albumType);
                cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapReaderToPopularRelease(reader));
                }

                _logger.LogDebug("Retrieved {Count} {AlbumType} releases from cache", list.Count, albumType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached releases for {AlbumType}", albumType);
            }
            return list;
        }

        public async Task<List<PopularRelease>> GetCachedSearchResultsAsync(
            string query, int pageNumber, int pageSize)
        {
            var list = new List<PopularRelease>();
            try
            {
                using var conn = _connection.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT * FROM PopularRelease
                    WHERE query = @query
                    ORDER BY popularity_score DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

                cmd.Parameters.AddWithValue("@query", query);
                cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapReaderToPopularRelease(reader));
                }

                _logger.LogDebug("Retrieved {Count} search results for '{Query}' from cache", list.Count, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached search results for '{Query}'", query);
            }
            return list;
        }

        public async Task SaveReleasesAsync(List<PopularRelease> releases)
        {
            if (releases == null || !releases.Any())
                return;

            try
            {
                using var conn = _connection.GetConnection();
                foreach (var r in releases)
                {
                    var cmd = new SqlCommand(@"MERGE INTO PopularRelease AS target
                        USING (
                          SELECT
                            @album_id    AS album_id,
                            @album_type  AS album_type,
                            @query       AS query
                        ) AS source
                          ON target.album_id   = source.album_id
                         AND target.album_type = source.album_type
                         AND (
                              (source.query IS NULL    AND target.query IS NULL)
                           OR (source.query = target.query)
                         )
                        WHEN MATCHED THEN
                          UPDATE SET
                            name             = @name,
                            artist           = @artist,
                            cover            = @cover,
                            release_date     = @release_date,
                            popularity_score = @popularity_score,
                            last_updated     = @last_updated,
                            is_available     = @is_available
                        WHEN NOT MATCHED THEN
                          INSERT (
                            album_id, album_type, query,
                            name, artist, cover,
                            release_date, popularity_score,
                            last_updated, is_available
                          )
                          VALUES (
                            @album_id, @album_type, @query,
                            @name, @artist, @cover,
                            @release_date, @popularity_score,
                            @last_updated, @is_available
                          );
                        );", conn);

                    AddParameters(cmd, r, query: null);
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("Saved {Count} releases to cache", releases.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving releases to cache");
                throw;
            }
        }

        public async Task<bool> IsCacheExpiredAsync(string albumType)
        {
            try
            {
                using var conn = _connection.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT MAX(last_updated)
                    FROM PopularRelease
                    WHERE album_type = @type
                    AND query IS NULL", conn);

                cmd.Parameters.AddWithValue("@type", albumType);
                var result = await cmd.ExecuteScalarAsync();

                if (result == DBNull.Value)
                {
                    _logger.LogDebug("No cache found for {AlbumType}", albumType);
                    return true;
                }

                var last = Convert.ToDateTime(result);
                var isExpired = (DateTime.UtcNow - last).TotalHours > 24;
                _logger.LogDebug("Cache for {AlbumType} last updated at {LastUpdated} - expired: {IsExpired}",
                    albumType, last, isExpired);

                return isExpired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache expiration for {AlbumType}", albumType);
                return true;
            }
        }

        public async Task SaveSearchResultsAsync(string query, List<PopularRelease> results)
        {
            if (results == null || !results.Any())
                return;

            try
            {
                using var conn = _connection.GetConnection();
                foreach (var r in results)
                {
                    var cmd = new SqlCommand(@"
                        MERGE INTO PopularRelease AS target
                        USING (SELECT @album_id AS album_id, @query AS query) AS source
                          ON target.album_id = source.album_id
                         AND target.query = source.query
                        WHEN MATCHED THEN
                          UPDATE SET
                            name = @name,
                            artist = @artist,
                            cover = @cover,
                            release_date = @release_date,
                            popularity_score = @popularity_score,
                            album_type = @album_type,
                            last_updated = @last_updated,
                            is_available = @is_available
                        WHEN NOT MATCHED THEN
                          INSERT (album_id, name, artist, cover,
                                  release_date, popularity_score,
                                  album_type, last_updated, is_available,
                                  query)
                          VALUES (@album_id, @name, @artist, @cover,
                                  @release_date, @popularity_score,
                                  @album_type, @last_updated,
                                  @is_available, @query);", conn);

                    AddParameters(cmd, r, query);
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("Saved {Count} search results for '{Query}' to cache", results.Count, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving search results for '{Query}' to cache", query);
                throw;
            }
        }

        public async Task<bool> IsSearchCacheExpiredAsync(string query)
        {
            try
            {
                using var conn = _connection.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT MAX(last_updated)
                    FROM PopularRelease
                    WHERE query = @query", conn);

                cmd.Parameters.AddWithValue("@query", query);
                var result = await cmd.ExecuteScalarAsync();

                if (result == DBNull.Value)
                {
                    _logger.LogDebug("No search cache found for '{Query}'", query);
                    return true;
                }

                var last = Convert.ToDateTime(result);
                var isExpired = (DateTime.UtcNow - last).TotalHours > 24;
                _logger.LogDebug("Search cache for '{Query}' last updated at {LastUpdated} - expired: {IsExpired}",
                    query, last, isExpired);

                return isExpired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking search cache expiration for '{Query}'", query);
                return true;
            }
        }

        private PopularRelease MapReaderToPopularRelease(SqlDataReader r)
        {
            var ord = new Func<string, int>(c => r.GetOrdinal(c));

            return new PopularRelease
            {
                AlbumId = r.GetString(ord("album_id")),
                Name = r.GetString(ord("name")),
                Artist = r.GetString(ord("artist")),
                Cover = r.GetString(ord("cover")),
                ReleaseDate = r.IsDBNull(ord("release_date"))
                                     ? DateTime.MinValue
                                     : r.GetDateTime(ord("release_date")),
                PopularityScore = r.GetInt32(ord("popularity_score")),
                AlbumType = r.GetString(ord("album_type")),
                LastUpdated = r.GetDateTime(ord("last_updated")),
                IsAvailable = r.GetBoolean(ord("is_available")),
                Query = r.IsDBNull(ord("query"))
                                     ? null
                                     : r.GetString(ord("query"))
            };
        }

        private void AddParameters(SqlCommand cmd, PopularRelease r, string? query)
        {
            cmd.Parameters.AddWithValue("@album_id", r.AlbumId);
            cmd.Parameters.AddWithValue("@name", r.Name);
            cmd.Parameters.AddWithValue("@artist", r.Artist);
            cmd.Parameters.AddWithValue("@cover", r.Cover);
            cmd.Parameters.AddWithValue("@release_date", r.ReleaseDate);
            cmd.Parameters.AddWithValue("@popularity_score", r.PopularityScore);
            cmd.Parameters.AddWithValue("@album_type", r.AlbumType);
            cmd.Parameters.AddWithValue("@last_updated", r.LastUpdated);
            cmd.Parameters.AddWithValue("@is_available", r.IsAvailable);
            cmd.Parameters.AddWithValue("@query", (object?)query ?? DBNull.Value);
        }
    }
}