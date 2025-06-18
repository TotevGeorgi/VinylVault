using Common.DTOs;
using Common.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DBOrder : IOrderRepository
    {
        private readonly DBConnection _dbConnection;

        public DBOrder(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> AddOrder(string buyerEmail, int vinylId)
        {
            using var conn = _dbConnection.GetConnection();
            try
            {
                string query = "INSERT INTO Orders (BuyerEmail, VinylId) VALUES (@BuyerEmail, @VinylId)";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BuyerEmail", buyerEmail);
                cmd.Parameters.AddWithValue("@VinylId", vinylId);

                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error adding order: {ex.Message}");
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetOrdersByUser(string email)
        {
            var orders = new List<OrderDTO>();

            using var conn = _dbConnection.GetConnection();
            try
            {
                string query = @"
                SELECT o.Id, o.BuyerEmail, o.VinylId, o.PurchaseDate,
                       v.Title, v.Artist
                FROM Orders o
                INNER JOIN Vinyls v ON o.VinylId = v.Id
                WHERE o.BuyerEmail = @Email
                ORDER BY o.PurchaseDate DESC";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    orders.Add(new OrderDTO
                    {
                        Id = reader.GetInt32(0),
                        BuyerEmail = reader.GetString(1),
                        VinylId = reader.GetInt32(2),
                        PurchaseDate = reader.GetDateTime(3),
                        Title = reader.GetString(4),
                        Artist = reader.GetString(5)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error retrieving orders: {ex.Message}");
            }

            return orders;
        }

    }
}
