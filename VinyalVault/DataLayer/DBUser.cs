using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Common.DTOs;
using System.Threading.Tasks;
using Common;

namespace DataLayer
{
    public class DBUser
    {
        private readonly DBConnection _dbConnection;

        public DBUser(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Register a user (async)
        public async Task<bool> RegisterUser(Person person)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "INSERT INTO Users (email, password_hash, full_name, address, remember_me, role) " +
                                   "VALUES (@Email, @PasswordHash, @FullName, @Address, @RememberMe, @Role)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", person.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", person.PasswordHash);
                        cmd.Parameters.AddWithValue("@FullName", person.FullName);
                        cmd.Parameters.AddWithValue("@Address", person.Address);
                        cmd.Parameters.AddWithValue("@RememberMe", person.RememberMe ? 1 : 0);
                        cmd.Parameters.AddWithValue("@Role", person.Role ?? "User");

                        int result = await cmd.ExecuteNonQueryAsync();  // Async method for DB insert
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error during user registration: {ex.Message}");
                    return false;
                }
            }
        }

        // Check if an email exists in the database (async)
        public async Task<bool> EmailExists(string email)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)await cmd.ExecuteScalarAsync();  // Async method for checking email
                        return count > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error checking email: {ex.Message}");
                    return false;
                }
            }
        }

        // Get user by email (async)
        public async Task<Person?> GetUserByEmail(string email)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "SELECT email, password_hash, full_name, address, role FROM Users WHERE email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())  // Async method to fetch user data
                        {
                            if (reader.Read())
                            {
                                Person person = new User  // Return as a User (could be Seller, Admin)
                                {
                                    Email = reader.GetString(0),
                                    PasswordHash = reader.GetString(1),
                                    FullName = reader.GetString(2),
                                    Address = reader.GetString(3),
                                    Role = reader.GetString(4)
                                };
                                return person;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error retrieving user: {ex.Message}");
                }
            }
            return null;
        }

        // Update user profile (sync)
        public bool UpdateUserProfile(string email, string fullName, string address)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                string query = "UPDATE Users SET full_name = @FullName, address = @Address WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.ExecuteNonQuery();  // Sync method for DB update
                    return true;
                }
            }
        }

        // Upgrade user to seller (async)
        public async Task<bool> UpgradeToSeller(string email)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "UPDATE Users SET role = 'Seller' WHERE email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int result = await cmd.ExecuteNonQueryAsync();  // Async method for DB update
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error upgrading user to seller: {ex.Message}");
                    return false;
                }
            }
        }

        // Delete a user (async)
        public async Task<bool> DeleteUser(string email)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "DELETE FROM Users WHERE email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int result = await cmd.ExecuteNonQueryAsync();  // Async method for DB delete
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error deleting user: {ex.Message}");
                    return false;
                }
            }
        }

        // Remove vinyl item (async)
        public async Task<bool> RemoveItem(string email, string item)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                try
                {
                    string query = "DELETE FROM Vinyls WHERE seller_email = @Email AND title = @Item";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Item", item);
                        int result = await cmd.ExecuteNonQueryAsync();  // Async method for DB delete
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error removing item: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
