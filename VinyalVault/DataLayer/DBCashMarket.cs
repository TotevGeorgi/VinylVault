using Common.DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DBCashMarket
    {
        private readonly DBConnection _connection;

        public DBCashMarket(DBConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<PopularRelease>> GetCachedReleasesAsync(string albumType)
        {
            var list = new List<PopularRelease>();

            using var conn = _connection.GetConnection();

            var cmd = new SqlCommand("SELECT * FROM PopularRelease WHERE album_type = @type", conn);
            cmd.Parameters.AddWithValue("@type", albumType);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new PopularRelease
                {
                    AlbumId = reader["album_id"].ToString(),
                    Name = reader["name"].ToString(),
                    Artist = reader["artist"].ToString(),
                    Cover = reader["cover"].ToString(),
                    ReleaseDate = Convert.ToDateTime(reader["release_date"]),
                    PopularityScore = Convert.ToInt32(reader["popularity_score"]),
                    AlbumType = reader["album_type"].ToString(),
                    LastUpdated = Convert.ToDateTime(reader["last_updated"])
                });
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
                    USING (SELECT @album_id AS album_id) AS source
                    ON target.album_id = source.album_id
                    WHEN MATCHED THEN 
                        UPDATE SET 
                            name = @name,
                            artist = @artist,
                            cover = @cover,
                            release_date = @release_date,
                            popularity_score = @popularity_score,
                            album_type = @album_type,
                            last_updated = @last_updated
                    WHEN NOT MATCHED THEN 
                        INSERT (album_id, name, artist, cover, release_date, popularity_score, album_type, last_updated)
                        VALUES (@album_id, @name, @artist, @cover, @release_date, @popularity_score, @album_type, @last_updated);", conn);
                    
                cmd.Parameters.AddWithValue("@album_id", r.AlbumId);
                cmd.Parameters.AddWithValue("@name", r.Name);
                cmd.Parameters.AddWithValue("@artist", r.Artist);
                cmd.Parameters.AddWithValue("@cover", r.Cover);
                cmd.Parameters.AddWithValue("@release_date", r.ReleaseDate);
                cmd.Parameters.AddWithValue("@popularity_score", r.PopularityScore);
                cmd.Parameters.AddWithValue("@album_type", r.AlbumType);
                cmd.Parameters.AddWithValue("@last_updated", r.LastUpdated);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> IsCacheExpiredAsync(string albumType)
        {
            using var conn = _connection.GetConnection();
            var cmd = new SqlCommand("SELECT MAX(last_updated) FROM PopularRelease WHERE album_type = @type", conn);
            cmd.Parameters.AddWithValue("@type", albumType);

            var result = await cmd.ExecuteScalarAsync();
            if (result == DBNull.Value) return true;

            return (DateTime.UtcNow - Convert.ToDateTime(result)).TotalHours > 24;
        }
    }
}
