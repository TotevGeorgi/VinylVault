using Common.DTOs;
using Common.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DBRating : IRatingRepository
    {
        private readonly DBConnection _db;

        public DBRating(DBConnection db)
        {
            _db = db;
        }

        public async Task<bool> AddRatingAsync(SellerRatingDTO rating)
        {
            using var conn = _db.GetConnection();
            try
            {
                var query = @"INSERT INTO SellerRatings 
            (BuyerEmail, SellerEmail, VinylId, Rating, Comment, CreatedAt) 
            VALUES (@BuyerEmail, @SellerEmail, @VinylId, @Rating, @Comment, @CreatedAt)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BuyerEmail", rating.BuyerEmail);
                cmd.Parameters.AddWithValue("@SellerEmail", rating.SellerEmail);
                cmd.Parameters.AddWithValue("@VinylId", rating.VinylId);
                cmd.Parameters.AddWithValue("@Rating", rating.Rating);
                cmd.Parameters.AddWithValue("@Comment", (object?)rating.Comment ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding rating: " + ex.Message);
                return false;
            }
        }


        public async Task<List<SellerRatingDTO>> GetRatingsForSellerAsync(string sellerEmail)
        {
            var result = new List<SellerRatingDTO>();
            using var conn = _db.GetConnection();

            try
            {
                string query = @"SELECT RatingId, BuyerEmail, SellerEmail, VinylId, Rating, Comment, CreatedAt 
                                 FROM SellerRatings WHERE SellerEmail = @SellerEmail";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SellerEmail", sellerEmail);

                using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    result.Add(new SellerRatingDTO
                    {
                        RatingId = reader.GetGuid(0),
                        BuyerEmail = reader.GetString(1),
                        SellerEmail = reader.GetString(2),
                        VinylId = reader.GetInt32(3),
                        Rating = reader.GetInt32(4),
                        Comment = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CreatedAt = reader.GetDateTime(6)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading ratings: " + ex.Message);
            }

            return result;
        }
    }
}
