using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Common.DTOs;
using Common;

namespace DataLayer
{
    public class DBVinyl
    {
        private readonly DBConnection _dbConnection;

        public DBVinyl(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> SaveVinyl(Vinyl vinyl)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "INSERT INTO Vinyls (AlbumId, Title, Artist, Condition, Description, ImagePath, SellerEmail, Status) " +
                                   "VALUES (@AlbumId, @Title, @Artist, @Condition, @Description, @ImagePath, @SellerEmail, @Status)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AlbumId", vinyl.AlbumId);
                        cmd.Parameters.AddWithValue("@Title", vinyl.Title);
                        cmd.Parameters.AddWithValue("@Artist", vinyl.Artist);
                        cmd.Parameters.AddWithValue("@Condition", vinyl.Condition);
                        cmd.Parameters.AddWithValue("@Description", vinyl.Description);
                        cmd.Parameters.AddWithValue("@ImagePath", vinyl.ImagePath);
                        cmd.Parameters.AddWithValue("@SellerEmail", vinyl.SellerEmail);
                        cmd.Parameters.AddWithValue("@Status", vinyl.Status);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error saving vinyl: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<List<Vinyl>> GetVinylsBySeller(string sellerEmail)
        {
            var vinyls = new List<Vinyl>();

            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "SELECT Id, AlbumId, Title, Artist, Condition, Description, ImagePath, SellerEmail, Status " +
                                   "FROM Vinyls WHERE SellerEmail = @SellerEmail";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SellerEmail", sellerEmail);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                vinyls.Add(new Vinyl
                                {
                                    Id = reader.GetInt32(0),
                                    AlbumId = reader.GetString(1),
                                    Title = reader.GetString(2),
                                    Artist = reader.GetString(3),
                                    Condition = reader.GetString(4),
                                    Description = reader.GetString(5),
                                    ImagePath = reader.GetString(6),
                                    SellerEmail = reader.GetString(7),
                                    Status = reader.GetString(8)
                                });
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error retrieving vinyls: {ex.Message}");
                }
            }

            return vinyls;
        }

        public async Task<bool> DeleteVinyl(int id)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "DELETE FROM Vinyls WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error deleting vinyl: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> UpdateVinyl(Vinyl vinyl)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "UPDATE Vinyls SET Title = @Title, Artist = @Artist, Condition = @Condition, " +
                                   "Description = @Description, ImagePath = @ImagePath, Status = @Status WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", vinyl.Title);
                        cmd.Parameters.AddWithValue("@Artist", vinyl.Artist);
                        cmd.Parameters.AddWithValue("@Condition", vinyl.Condition);
                        cmd.Parameters.AddWithValue("@Description", vinyl.Description);
                        cmd.Parameters.AddWithValue("@ImagePath", vinyl.ImagePath);
                        cmd.Parameters.AddWithValue("@Status", vinyl.Status);
                        cmd.Parameters.AddWithValue("@Id", vinyl.Id);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error updating vinyl: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<List<Vinyl>> GetVinylsByAlbumIdAndStatus(string albumId, string status)
        {
            var list = new List<Vinyl>();
            using (var conn = _dbConnection.GetConnection())
            {
                string query = "SELECT Id, Title, Artist, Condition, Description, ImagePath, SellerEmail, Status, AlbumId " +
                               "FROM Vinyls WHERE AlbumId = @AlbumId AND Status = @Status";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AlbumId", albumId);
                    cmd.Parameters.AddWithValue("@Status", status);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Vinyl
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Artist = reader.GetString(2),
                                Condition = reader.GetString(3),
                                Description = reader.GetString(4),
                                ImagePath = reader.GetString(5),
                                SellerEmail = reader.GetString(6),
                                Status = reader.GetString(7),
                                AlbumId = reader.GetString(8)
                            });
                        }
                    }
                }
            }
            return list;
        }


        public async Task<bool> UpdateStatus(int id, string status)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                string query = "UPDATE Vinyls SET Status = @Status WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Id", id);
                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }
    }
}
