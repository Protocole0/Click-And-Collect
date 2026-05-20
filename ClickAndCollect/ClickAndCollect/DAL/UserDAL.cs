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

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT u.user_id, u.user_type, u.password, u.email, " +
                "       c.client_id, c.first_name, c.last_name, c.phone_number, " +
                "       e.store_id, s.name AS store_name, s.street_name, s.street_number, s.city, s.postal_code " +
                "FROM users u " +
                "LEFT JOIN client   c ON u.user_id  = c.user_id " +
                "LEFT JOIN employee e ON u.user_id  = e.user_id " +
                "LEFT JOIN store    s ON e.store_id = s.store_id " +
                "WHERE u.email = @email",
                conn);
            cmd.Parameters.AddWithValue("@email", email);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            string storedHash = reader.GetString(reader.GetOrdinal("password"));
            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                return null;

            string userType = reader.GetString(reader.GetOrdinal("user_type"));
            string userEmail = reader.GetString(reader.GetOrdinal("email"));
            int    userId   = reader.GetInt32(reader.GetOrdinal("user_id"));

            User user;

            if (userType == "client")
            {
                int    clientId  = reader.GetInt32(reader.GetOrdinal("client_id"));
                string firstname = reader.GetString(reader.GetOrdinal("first_name"));
                string lastname  = reader.GetString(reader.GetOrdinal("last_name"));
                string phone     = reader.IsDBNull(reader.GetOrdinal("phone_number"))
                                   ? string.Empty
                                   : reader.GetString(reader.GetOrdinal("phone_number"));

                var client = new Client(clientId, firstname, lastname, phone);
                client.Email = userEmail;
                user = client;
            }
            else
            {
                var store = new Store(
                    reader.GetInt32(reader.GetOrdinal("store_id")),
                    reader.GetString(reader.GetOrdinal("store_name")),
                    reader.GetString(reader.GetOrdinal("street_name")),
                    reader.GetString(reader.GetOrdinal("street_number")),
                    reader.GetString(reader.GetOrdinal("city")),
                    reader.GetString(reader.GetOrdinal("postal_code")));

                user = userType == "cashier"
                    ? new Cashier(userId, userEmail, store)
                    : new OrderPicker(userId, userEmail, store);
            }

            user.UserType = userType;
            return user;
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
