using Common.DTOs;
using Common.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DbCacheMarket : ICacheRepository
    {
        private readonly DBConnection _connection;

        public DbCacheMarket(DBConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<PopularRelease>> GetCachedReleasesAsync(
            string albumType, int pageNumber, int pageSize)
        {
            var list = new List<PopularRelease>();
            using var conn = _connection.GetConnection();

            var cmd = new SqlCommand(@"
                SELECT * FROM PopularRelease
                WHERE album_type = @type AND query IS NULL
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
            return list;
        }

        public async Task<List<PopularRelease>> GetCachedSearchResultsAsync(
            string query, int pageNumber, int pageSize)
        {
            var list = new List<PopularRelease>();
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
            return list;
        }

        public async Task SaveReleasesAsync(List<PopularRelease> releases)
        {
            using var conn = _connection.GetConnection();
            foreach (var r in releases)
            {
                var cmd = new SqlCommand(@"
                    MERGE INTO PopularRelease AS target
                    USING (SELECT @album_id AS album_id, NULL AS query) AS source
                      ON target.album_id = source.album_id
                     AND target.query IS NULL
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
                      INSERT (album_id, name, artist, cover, release_date,
                              popularity_score, album_type, last_updated,
                              is_available, query)
                      VALUES (@album_id, @name, @artist, @cover,
                              @release_date, @popularity_score,
                              @album_type, @last_updated,
                              @is_available, NULL);", conn);

                AddParameters(cmd, r, query: null);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> IsCacheExpiredAsync(string albumType)
        {
            using var conn = _connection.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT MAX(last_updated)
                  FROM PopularRelease
                 WHERE album_type = @type
                   AND query IS NULL", conn);

            cmd.Parameters.AddWithValue("@type", albumType);
            var result = await cmd.ExecuteScalarAsync();
            if (result == DBNull.Value) return true;

            var last = Convert.ToDateTime(result);
            return (DateTime.UtcNow - last).TotalHours > 24;
        }

        public async Task SaveSearchResultsAsync(string query, List<PopularRelease> results)
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
        }

        public async Task<bool> IsSearchCacheExpiredAsync(string query)
        {
            using var conn = _connection.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT MAX(last_updated)
                  FROM PopularRelease
                 WHERE query = @query", conn);

            cmd.Parameters.AddWithValue("@query", query);
            var result = await cmd.ExecuteScalarAsync();
            if (result == DBNull.Value) return true;

            var last = Convert.ToDateTime(result);
            return (DateTime.UtcNow - last).TotalHours > 24;
        }

        private PopularRelease MapReaderToPopularRelease(SqlDataReader reader)
        {
            return new PopularRelease
            {
                AlbumId = reader["album_id"].ToString(),
                Name = reader["name"].ToString(),
                Artist = reader["artist"].ToString(),
                Cover = reader["cover"].ToString(),
                ReleaseDate = Convert.ToDateTime(reader["release_date"]),
                PopularityScore = Convert.ToInt32(reader["popularity_score"]),
                AlbumType = reader["album_type"].ToString(),
                LastUpdated = Convert.ToDateTime(reader["last_updated"]),
                IsAvailable = Convert.ToBoolean(reader["is_available"]),
                Query = reader["query"] as string
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
