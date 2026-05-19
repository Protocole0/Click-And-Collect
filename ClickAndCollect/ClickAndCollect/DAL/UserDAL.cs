using BCrypt.Net;
using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly string _connectionString;

        public UserDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Client?> GetByEmailAndPasswordAsync(string email, string password)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT u.user_id, u.user_type, u.password, u.email, c.first_name, c.last_name, c.phone_number " +
                "FROM users u " +
                "LEFT JOIN Client c ON u.user_id = c.user_id " +
                "WHERE u.email = @email",
                conn);
            cmd.Parameters.AddWithValue("@email", email);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            string storedHash = reader.GetString(reader.GetOrdinal("password"));
            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                return null;

            int    idOrd        = reader.GetOrdinal("user_id");
            int    typeOrd      = reader.GetOrdinal("user_type");
            int    emailOrd     = reader.GetOrdinal("email");
            int    firstnameOrd = reader.GetOrdinal("first_name");
            int    lastnameOrd  = reader.GetOrdinal("last_name");
            int    phoneOrd     = reader.GetOrdinal("phone_number");

            string firstname = reader.IsDBNull(firstnameOrd) ? string.Empty : reader.GetString(firstnameOrd);
            string lastname  = reader.IsDBNull(lastnameOrd)  ? string.Empty : reader.GetString(lastnameOrd);
            string phone     = reader.IsDBNull(phoneOrd)     ? string.Empty : reader.GetString(phoneOrd);

            var client = new Client(reader.GetInt32(idOrd), firstname, lastname, phone);
            client.UserType = reader.GetString(typeOrd);
            client.Email    = reader.GetString(emailOrd);
            return client;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(1) FROM users WHERE email = @email", conn);
            cmd.Parameters.AddWithValue("@email", email);
            int count = (int)(await cmd.ExecuteScalarAsync())!;
            return count > 0;
        }

        public async Task CreateAsync(Client client)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(client.Password);

            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using SqlTransaction tx = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                SqlCommand cmdUser = new SqlCommand(
                    "INSERT INTO users (email, password, user_type) OUTPUT INSERTED.user_id " +
                    "VALUES (@email, @password, 'client')",
                    conn, tx);
                cmdUser.Parameters.AddWithValue("@email",    client.Email);
                cmdUser.Parameters.AddWithValue("@password", hashedPassword);
                int newId = (int)(await cmdUser.ExecuteScalarAsync())!;

                SqlCommand cmdClient = new SqlCommand(
                    "INSERT INTO Client (user_id, first_name, last_name, phone_number) " +
                    "VALUES (@userId, @firstname, @lastname, @phone)",
                    conn, tx);
                cmdClient.Parameters.AddWithValue("@userId",    newId);
                cmdClient.Parameters.AddWithValue("@firstname", client.Firstname);
                cmdClient.Parameters.AddWithValue("@lastname",  client.Lastname);
                cmdClient.Parameters.AddWithValue("@phone",     client.PhoneNumber);
                await cmdClient.ExecuteNonQueryAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
