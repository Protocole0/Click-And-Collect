using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class CategoryDal : ICategoryDal
    {
        private readonly string _connectionString;

        public CategoryDal(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Category> GetAll()
        {
            List<Category> categories = new List<Category>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT category_id, name, image_url, description FROM category ORDER BY name",
                    conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int categoryIdOrd = reader.GetOrdinal("category_id");
                    int nameOrd       = reader.GetOrdinal("name");
                    int imageUrlOrd   = reader.GetOrdinal("image_url");
                    int descOrd       = reader.GetOrdinal("description");

                    while (reader.Read())
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
