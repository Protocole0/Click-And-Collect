using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class StoreDAL : IStoreDAL
    {
        private readonly string _connectionString;

        public StoreDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Store>> GetAllAsync()
        {
            List<Store> stores = new List<Store>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT store_id, name, street_name, street_number, city, postal_code FROM store ORDER BY city, name",
                conn);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            int idOrd     = reader.GetOrdinal("store_id");
            int nameOrd   = reader.GetOrdinal("name");
            int streetOrd = reader.GetOrdinal("street_name");
            int numOrd    = reader.GetOrdinal("street_number");
            int cityOrd   = reader.GetOrdinal("city");
            int zipOrd    = reader.GetOrdinal("postal_code");

            while (await reader.ReadAsync())
            {
                stores.Add(new Store(
                    reader.GetInt32(idOrd),
                    reader.GetString(nameOrd),
                    reader.GetString(streetOrd),
                    reader.GetString(numOrd),
                    reader.GetString(cityOrd),
                    reader.GetString(zipOrd)));
            }

            return stores;
        }

        public async Task<Store?> GetByIdAsync(int storeId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT store_id, name, street_name, street_number, city, postal_code FROM store WHERE store_id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", storeId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Store(
                    reader.GetInt32(reader.GetOrdinal("store_id")),
                    reader.GetString(reader.GetOrdinal("name")),
                    reader.GetString(reader.GetOrdinal("street_name")),
                    reader.GetString(reader.GetOrdinal("street_number")),
                    reader.GetString(reader.GetOrdinal("city")),
                    reader.GetString(reader.GetOrdinal("postal_code")));
            }
            return null;
        }
    }
}
