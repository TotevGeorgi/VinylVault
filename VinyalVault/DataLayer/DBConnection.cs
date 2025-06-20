using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DataLayer
{
    public class DBConnection
    {
        private readonly string _connectionString;

        public DBConnection()
        {
            _connectionString = "Server=mssqlstud.fhict.local;Database=dbi532075_vinylvault;User Id=dbi532075_vinylvault;Password=VinylVault;TrustServerCertificate=True;";
        }

        public DBConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            try
            {
                SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public void CloseConnection(SqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
