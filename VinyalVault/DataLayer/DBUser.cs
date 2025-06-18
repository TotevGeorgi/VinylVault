using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Common.DTOs;
using System.Threading.Tasks;
using Common.Repositories;
using Common;

namespace DataLayer
{
    public class DBUser : IUserRepository
    {
        private readonly DBConnection _dbConnection;

        public DBUser(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Guid?> RegisterUser(Person person)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                Guid userId = person.UserId == Guid.Empty ? Guid.NewGuid() : person.UserId;

                string query = @"INSERT INTO Users (user_id, email, password_hash, full_name, address, remember_me, role)
                                 VALUES (@UserId, @Email, @PasswordHash, @FullName, @Address, @RememberMe, @Role)";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Email", person.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", person.PasswordHash ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", person.FullName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Address", person.Address ?? string.Empty);
                cmd.Parameters.AddWithValue("@RememberMe", person.RememberMe ? 1 : 0);
                cmd.Parameters.AddWithValue("@Role", person.Role ?? "User");

                int result = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[DEBUG] DBUser.RegisterUser -> Inserted rows: {result}");
                return result > 0 ? userId : null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.RegisterUser -> SQL Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EmailExists(string email)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = "SELECT COUNT(*) FROM Users WHERE email = @Email";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                int count = (int)await cmd.ExecuteScalarAsync();

                Console.WriteLine($"[DEBUG] DBUser.EmailExists -> Email: {email}, Count: {count}");
                return count > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.EmailExists -> SQL Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<Person?> GetUserByEmail(string email)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = @"SELECT user_id, email, password_hash, full_name, address, role 
                                 FROM Users WHERE email = @Email";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    Console.WriteLine($"[DEBUG] DBUser.GetUserByEmail -> Found user for {email}");

                    return new User
                    {
                        UserId = reader.GetGuid(0),
                        Email = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Role = reader.IsDBNull(5) ? "User" : reader.GetString(5)
                    };
                }

                Console.WriteLine($"[DEBUG] DBUser.GetUserByEmail -> No user found for {email}");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.GetUserByEmail -> SQL Exception: {ex.Message}");
            }

            return null;
        }

        public bool UpdateUserProfile(string email, string fullName, string address)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = "UPDATE Users SET full_name = @FullName, address = @Address WHERE email = @Email";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Email", email);

                int affected = cmd.ExecuteNonQuery();
                Console.WriteLine($"[DEBUG] DBUser.UpdateUserProfile -> Updated rows: {affected}");
                return affected > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.UpdateUserProfile -> SQL Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpgradeToSeller(string email)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = "UPDATE Users SET role = 'Seller' WHERE email = @Email";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                int result = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[DEBUG] DBUser.UpgradeToSeller -> Result: {result}");
                return result > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.UpgradeToSeller -> SQL Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUser(string email)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = "DELETE FROM Users WHERE email = @Email";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                int result = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[DEBUG] DBUser.DeleteUser -> Result: {result}");
                return result > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.DeleteUser -> SQL Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveItem(string email, string item)
        {
            using SqlConnection conn = _dbConnection.GetConnection();
            try
            {
                string query = "DELETE FROM Vinyls WHERE seller_email = @Email AND title = @Item";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Item", item);
                int result = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[DEBUG] DBUser.RemoveItem -> Result: {result}");
                return result > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[ERROR] DBUser.RemoveItem -> SQL Exception: {ex.Message}");
                return false;
            }
        }
    }
}
