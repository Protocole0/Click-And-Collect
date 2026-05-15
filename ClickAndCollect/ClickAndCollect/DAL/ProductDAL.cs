using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class ProductDAL : IProductDAL
    {
        private readonly string _connectionString;

        public ProductDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    "SELECT p.product_id, p.name, p.description, p.price, p.image_url, p.nutritional_info, p.category_id, c.name AS category_name " +
                    "FROM product p JOIN category c ON p.category_id = c.category_id " +
                    "WHERE p.category_id = @categoryId ORDER BY p.name",
                    conn);
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int productIdOrd    = reader.GetOrdinal("product_id");
                    int nameOrd         = reader.GetOrdinal("name");
                    int descOrd         = reader.GetOrdinal("description");
                    int priceOrd        = reader.GetOrdinal("price");
                    int imageUrlOrd     = reader.GetOrdinal("image_url");
                    int nutritionalOrd  = reader.GetOrdinal("nutritional_info");
                    int categoryIdOrd   = reader.GetOrdinal("category_id");
                    int categoryNameOrd = reader.GetOrdinal("category_name");

                    while (await reader.ReadAsync())
                    {
                        var category = new Category(
                            reader.GetInt32(categoryIdOrd),
                            reader.GetString(categoryNameOrd),
                            null, null);

                        products.Add(new Product(
                            reader.GetInt32(productIdOrd),
                            reader.GetString(nameOrd),
                            reader.IsDBNull(descOrd)        ? null : reader.GetString(descOrd),
                            reader.GetDecimal(priceOrd),
                            reader.IsDBNull(imageUrlOrd)    ? null : reader.GetString(imageUrlOrd),
                            reader.IsDBNull(nutritionalOrd) ? null : reader.GetString(nutritionalOrd),
                            reader.GetInt32(categoryIdOrd),
                            category));
                    }
                }
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            Product? product = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    "SELECT p.product_id, p.name, p.description, p.price, p.image_url, p.nutritional_info, p.category_id, c.name AS category_name " +
                    "FROM product p " +
                    "JOIN category c ON p.category_id = c.category_id " +
                    "WHERE p.product_id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int productIdOrd    = reader.GetOrdinal("product_id");
                    int nameOrd         = reader.GetOrdinal("name");
                    int descOrd         = reader.GetOrdinal("description");
                    int priceOrd        = reader.GetOrdinal("price");
                    int imageUrlOrd     = reader.GetOrdinal("image_url");
                    int nutritionalOrd  = reader.GetOrdinal("nutritional_info");
                    int categoryIdOrd   = reader.GetOrdinal("category_id");
                    int categoryNameOrd = reader.GetOrdinal("category_name");

                    if (await reader.ReadAsync())
                    {
                        var category = new Category(
                            reader.GetInt32(categoryIdOrd),
                            reader.GetString(categoryNameOrd),
                            null, null);

                        product = new Product(
                            reader.GetInt32(productIdOrd),
                            reader.GetString(nameOrd),
                            reader.IsDBNull(descOrd)        ? null : reader.GetString(descOrd),
                            reader.GetDecimal(priceOrd),
                            reader.IsDBNull(imageUrlOrd)    ? null : reader.GetString(imageUrlOrd),
                            reader.IsDBNull(nutritionalOrd) ? null : reader.GetString(nutritionalOrd),
                            reader.GetInt32(categoryIdOrd),
                            category);
                    }
                }
            }

            return product;
        }
    }
}
