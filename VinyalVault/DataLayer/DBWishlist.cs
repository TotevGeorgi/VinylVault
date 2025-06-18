using Common.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DBWishlist : IWishlistRepository
    {
        private readonly DBConnection _dbConnection;

        public DBWishlist(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> ExistsByAlbum(string userId, string albumId)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT COUNT(*) FROM Wishlist WHERE user_id = @UserId AND external_album_id = @AlbumId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@AlbumId", albumId);

            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<bool> AddByAlbum(string userId, string albumId)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "INSERT INTO Wishlist (user_id, external_album_id) VALUES (@UserId, @AlbumId)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@AlbumId", albumId);

            try
            {
                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Failed to add to wishlist: {ex.Message}");
                return false;
            }
        }
        public async Task<List<string>> GetAlbumIdsInWishlist(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must not be null or empty", nameof(userId));

            var albumIds = new List<string>();

            using var conn = _dbConnection.GetConnection();
            string query = @"
        SELECT external_album_id 
        FROM Wishlist 
        WHERE user_id = @UserId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                albumIds.Add(reader.GetString(0));
            }

            return albumIds;
        }


    }

}
