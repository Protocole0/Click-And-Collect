using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class CategoryDAL : ICategoryDAL
    {
        private readonly string _connectionString;

        public CategoryDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT category_id, name, image_url, description FROM category WHERE category_id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", categoryId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new Category(
                reader.GetInt32(reader.GetOrdinal("category_id")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url")),
                reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description"))
            );
        }

        public async Task<List<Category>> GetAllAsync()
        {
            List<Category> categories = new List<Category>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    "SELECT category_id, name, image_url, description FROM category ORDER BY name",
                    conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int categoryIdOrd = reader.GetOrdinal("category_id");
                    int nameOrd       = reader.GetOrdinal("name");
                    int imageUrlOrd   = reader.GetOrdinal("image_url");
                    int descOrd       = reader.GetOrdinal("description");

                    while (await reader.ReadAsync())
                    {
                        categories.Add(new Category(
                            reader.GetInt32(categoryIdOrd),
                            reader.GetString(nameOrd),
                            reader.IsDBNull(imageUrlOrd) ? null : reader.GetString(imageUrlOrd),
                            reader.IsDBNull(descOrd)     ? null : reader.GetString(descOrd)
                        ));
                    }
                }
            }

            return categories;
        }
    }
}
