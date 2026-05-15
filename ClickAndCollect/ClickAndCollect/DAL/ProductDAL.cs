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

        public List<Product> GetByCategoryId(int categoryId)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT product_id, name, description, price, image_url, nutritional_info, category_id " +
                    "FROM product WHERE category_id = @categoryId ORDER BY name",
                    conn);
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int productIdOrd   = reader.GetOrdinal("product_id");
                    int nameOrd        = reader.GetOrdinal("name");
                    int descOrd        = reader.GetOrdinal("description");
                    int priceOrd       = reader.GetOrdinal("price");
                    int imageUrlOrd    = reader.GetOrdinal("image_url");
                    int nutritionalOrd = reader.GetOrdinal("nutritional_info");
                    int categoryIdOrd  = reader.GetOrdinal("category_id");

                    while (reader.Read())
                    {
                        products.Add(new Product(
                            reader.GetInt32(productIdOrd),
                            reader.GetString(nameOrd),
                            reader.IsDBNull(descOrd)        ? null : reader.GetString(descOrd),
                            reader.GetDecimal(priceOrd),
                            reader.IsDBNull(imageUrlOrd)    ? null : reader.GetString(imageUrlOrd),
                            reader.IsDBNull(nutritionalOrd) ? null : reader.GetString(nutritionalOrd),
                            reader.GetInt32(categoryIdOrd)
                        ));
                    }
                }
            }

            return products;
        }

        public Product? GetById(int id)
        {
            Product? product = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT product_id, name, description, price, image_url, nutritional_info, category_id " +
                    "FROM product WHERE product_id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int productIdOrd   = reader.GetOrdinal("product_id");
                    int nameOrd        = reader.GetOrdinal("name");
                    int descOrd        = reader.GetOrdinal("description");
                    int priceOrd       = reader.GetOrdinal("price");
                    int imageUrlOrd    = reader.GetOrdinal("image_url");
                    int nutritionalOrd = reader.GetOrdinal("nutritional_info");
                    int categoryIdOrd  = reader.GetOrdinal("category_id");

                    if (reader.Read())
                    {
                        product = new Product(
                            reader.GetInt32(productIdOrd),
                            reader.GetString(nameOrd),
                            reader.IsDBNull(descOrd)        ? null : reader.GetString(descOrd),
                            reader.GetDecimal(priceOrd),
                            reader.IsDBNull(imageUrlOrd)    ? null : reader.GetString(imageUrlOrd),
                            reader.IsDBNull(nutritionalOrd) ? null : reader.GetString(nutritionalOrd),
                            reader.GetInt32(categoryIdOrd)
                        );
                    }
                }
            }

            return product;
        }
    }
}
