using Common.DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer
{
    public class DBOrder
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

        public async Task<List<Order>> GetOrdersByUser(string email)
        {
            var orders = new List<Order>();

            using var conn = _dbConnection.GetConnection();
            try
            {
                string query = "SELECT Id, BuyerEmail, VinylId, PurchaseDate FROM Orders WHERE BuyerEmail = @Email";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        BuyerEmail = reader.GetString(1),
                        VinylId = reader.GetInt32(2),
                        PurchaseDate = reader.GetDateTime(3)
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
