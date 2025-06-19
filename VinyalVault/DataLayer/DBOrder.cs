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
        private readonly DBConnection _db;

        public DBOrder(DBConnection dbConnection)
            => _db = dbConnection;

        public async Task<bool> AddOrder(string buyerEmail, int vinylId)
        {
            using var conn = _db.GetConnection();
            const string sql = @"
                INSERT INTO dbo.OrderHistory
                   (UserEmail, VinylId, PurchaseDate, Price, Title, Artist, CoverUrl)
                SELECT
                   @UserEmail,
                   @VinylId,
                   GETDATE(),
                   v.Price,
                   v.Title,
                   v.Artist,
                   v.ImagePath  
                FROM dbo.Vinyls AS v
                WHERE v.Id = @VinylId;
                ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserEmail", buyerEmail);
            cmd.Parameters.AddWithValue("@VinylId", vinylId);

            try
            {
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[DBOrder.AddOrder] {ex.Message}");
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetOrdersByUser(string email)
        {
            var list = new List<OrderDTO>();
            using var conn = _db.GetConnection();
            const string sql = @"
                SELECT 
                   o.Id,
                   o.UserEmail,
                   o.VinylId,
                   o.PurchaseDate,
                   o.Price,
                   o.Title,
                   o.Artist,
                   o.CoverUrl
                FROM dbo.OrderHistory AS o
                WHERE o.UserEmail = @Email
                ORDER BY o.PurchaseDate DESC;
                ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new OrderDTO
                {
                    Id = rdr.GetInt32(0),
                    BuyerEmail = rdr.GetString(1),
                    VinylId = rdr.GetInt32(2),
                    PurchaseDate = rdr.GetDateTime(3),
                    Price = rdr.GetDecimal(4),
                    Title = rdr.GetString(5),
                    Artist = rdr.GetString(6),
                    CoverUrl = rdr.GetString(7)
                });
            }

            return list;
        }
    }
}
