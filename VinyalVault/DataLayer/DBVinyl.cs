using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Common;
using Common.DTOs;
using Common.Repositories;

namespace DataLayer
{
    public class DBVinyl : IVinylRepository
    {
        private readonly DBConnection _dbConnection;
        public DBVinyl(DBConnection dbConnection)
            => _dbConnection = dbConnection;

        public async Task<bool> SaveVinyl(Vinyl vinyl)
        {
            using var conn = _dbConnection.GetConnection();
            const string sql = @"
                INSERT INTO Vinyls
                  (AlbumId, Title, Artist,
                   Condition, Description, ImagePath,
                   SellerEmail, Status, Price, DateAdded)
                VALUES
                  (@AlbumId, @Title, @Artist,
                   @Condition, @Description, @ImagePath,
                   @SellerEmail, @Status, @Price, GETDATE())";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AlbumId", vinyl.AlbumId);
            cmd.Parameters.AddWithValue("@Title", vinyl.Title);
            cmd.Parameters.AddWithValue("@Artist", vinyl.Artist);
            cmd.Parameters.AddWithValue("@Condition", vinyl.Condition);
            cmd.Parameters.AddWithValue("@Description", vinyl.Description);
            cmd.Parameters.AddWithValue("@ImagePath", vinyl.ImagePath);
            cmd.Parameters.AddWithValue("@SellerEmail", vinyl.SellerEmail);
            cmd.Parameters.AddWithValue("@Status", vinyl.Status);
            cmd.Parameters.AddWithValue("@Price", vinyl.Price);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Vinyl>> GetVinylsBySeller(string sellerEmail)
        {
            var list = new List<Vinyl>();
            using var conn = _dbConnection.GetConnection();
            const string sql = @"
                SELECT 
                  Id,
                  AlbumId,
                  Title,
                  Artist,
                  Condition,
                  Description,
                  ImagePath,
                  SellerEmail,
                  Status,
                  Price,
                  DateAdded
                FROM Vinyls
                WHERE SellerEmail = @SellerEmail";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerEmail", sellerEmail);

            using var rdr = await cmd.ExecuteReaderAsync();
            while (rdr.Read())
            {
                list.Add(new Vinyl
                {
                    Id = rdr.GetInt32(0),
                    AlbumId = rdr.GetString(1),
                    Title = rdr.GetString(2),
                    Artist = rdr.GetString(3),
                    Condition = rdr.GetString(4),
                    Description = rdr.GetString(5),
                    ImagePath = rdr.GetString(6),
                    SellerEmail = rdr.GetString(7),
                    Status = rdr.GetString(8),
                    Price = rdr.GetDecimal(9),
                    DateAdded = rdr.IsDBNull(10)
                                  ? new DateTime(1999, 12, 19)
                                  : rdr.GetDateTime(10)
                });
            }
            return list;
        }

        public async Task<bool> DeleteVinyl(int id)
        {
            using var conn = _dbConnection.GetConnection();
            const string sql = @"DELETE FROM Vinyls WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateVinyl(Vinyl vinyl)
        {
            using var conn = _dbConnection.GetConnection();
            const string sql = @"
                UPDATE Vinyls
                   SET Title       = @Title,
                       Artist      = @Artist,
                       Condition   = @Condition,
                       Description = @Description,
                       ImagePath   = @ImagePath,
                       Status      = @Status,
                       Price       = @Price
                 WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", vinyl.Title);
            cmd.Parameters.AddWithValue("@Artist", vinyl.Artist);
            cmd.Parameters.AddWithValue("@Condition", vinyl.Condition);
            cmd.Parameters.AddWithValue("@Description", vinyl.Description);
            cmd.Parameters.AddWithValue("@ImagePath", vinyl.ImagePath);
            cmd.Parameters.AddWithValue("@Status", vinyl.Status);
            cmd.Parameters.AddWithValue("@Price", vinyl.Price);
            cmd.Parameters.AddWithValue("@Id", vinyl.Id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Vinyl>> GetVinylsByAlbumIdAndStatus(string albumId, string status)
        {
            var list = new List<Vinyl>();
            using var conn = _dbConnection.GetConnection();
            const string sql = @"
                SELECT 
                  Id,
                  Title,
                  Artist,
                  Condition,
                  Description,
                  ImagePath,
                  SellerEmail,
                  Status,
                  AlbumId,
                  Price,
                  DateAdded
                FROM Vinyls
                WHERE AlbumId = @AlbumId
                  AND Status  = @Status";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AlbumId", albumId);
            cmd.Parameters.AddWithValue("@Status", status);

            using var rdr = await cmd.ExecuteReaderAsync();
            while (rdr.Read())
            {
                list.Add(new Vinyl
                {
                    Id = rdr.GetInt32(0),
                    Title = rdr.GetString(1),
                    Artist = rdr.GetString(2),
                    Condition = rdr.GetString(3),
                    Description = rdr.GetString(4),
                    ImagePath = rdr.GetString(5),
                    SellerEmail = rdr.GetString(6),
                    Status = rdr.GetString(7),
                    AlbumId = rdr.GetString(8),
                    Price = rdr.GetDecimal(9),
                    DateAdded = rdr.IsDBNull(10)
                                  ? new DateTime(1999, 12, 19)
                                  : rdr.GetDateTime(10)
                });
            }
            return list;
        }

        public async Task<Vinyl?> GetVinylById(int id)
        {
            using var conn = _dbConnection.GetConnection();
            const string sql = @"
                SELECT 
                  Id,
                  AlbumId,
                  Title,
                  Artist,
                  Condition,
                  Description,
                  ImagePath,
                  SellerEmail,
                  Status,
                  Price,
                  DateAdded
                FROM Vinyls
                WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var rdr = await cmd.ExecuteReaderAsync();
            if (!rdr.Read()) return null;

            return new Vinyl
            {
                Id = rdr.GetInt32(0),
                AlbumId = rdr.GetString(1),
                Title = rdr.GetString(2),
                Artist = rdr.GetString(3),
                Condition = rdr.GetString(4),
                Description = rdr.GetString(5),
                ImagePath = rdr.GetString(6),
                SellerEmail = rdr.GetString(7),
                Status = rdr.GetString(8),
                Price = rdr.GetDecimal(9),
                DateAdded = rdr.IsDBNull(10)
                                  ? new DateTime(1999, 12, 19)
                                  : rdr.GetDateTime(10)
            };
        }

        public async Task<bool> UpdateStatus(int id, string status)
        {
            using var conn = _dbConnection.GetConnection();
            const string sql = @"UPDATE Vinyls SET Status = @Status WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
