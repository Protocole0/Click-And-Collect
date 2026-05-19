using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class OrderDAL : IOrderDAL
    {
        private readonly string _connectionString;

        public OrderDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Get all orders with a specific status, including client information
        public async Task<List<OrderViewModel>> GetAllOrdersAsync(OrderStatus status, int storeId)
        {
            List<OrderViewModel> orders = new List<OrderViewModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query =
                @"SELECT 
                   o.order_id, o.order_date, o.crates_used, o.crates_returned, o.status,
                   c.first_name, c.last_name, c.phone_number,
                   SUM(ol.quantity) AS total_items,
                   t.date_slot AS book_date, t.start_time AS book_start, t.end_time AS book_end
                FROM dbo.Orders o
                JOIN dbo.Client c ON o.user_id = c.user_id
                JOIN dbo.Order_line ol ON o.order_id = ol.order_id
                JOIN dbo.Time_slot t ON o.time_slot_id = t.time_slot_id
                WHERE o.status = @status
                AND o.store_id = @storeId
                AND CAST(t.date_slot AS DATE) = CAST(DATEADD(day, 1, GETDATE()) AS DATE)
                GROUP BY 
                   o.order_id, o.order_date, o.crates_used, o.crates_returned, o.status,
                   c.first_name, c.last_name, c.phone_number,
                   t.date_slot, t.start_time, t.end_time
                ORDER BY t.start_time ASC";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@status", Convert.ToString(status));
                cmd.Parameters.AddWithValue("@storeId", storeId);
                await connection.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int orderIdOrd = reader.GetOrdinal("order_id");
                    int orderDateOrd = reader.GetOrdinal("order_date");
                    int statusOrd = reader.GetOrdinal("status");
                    int clientFirstnameOrd = reader.GetOrdinal("first_name");
                    int clientLastnameOrd = reader.GetOrdinal("last_name");
                    int clientPhoneNumberOrd = reader.GetOrdinal("phone_number");
                    int totalItemsOrd = reader.GetOrdinal("total_items");
                    int bookDateOrd = reader.GetOrdinal("book_date");
                    int bookStartOrd = reader.GetOrdinal("book_start");
                    int bookEndOrd = reader.GetOrdinal("book_end");

                    while (await reader.ReadAsync())
                    {
                        orders.Add(new OrderViewModel
                        {
                            OrderId = reader.GetInt32(orderIdOrd),
                            OrderDate = reader.GetDateTime(orderDateOrd),
                            OrderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), reader.GetString(statusOrd), true),
                            ClientFirstname = reader.GetString(clientFirstnameOrd),
                            ClientLastname = reader.GetString(clientLastnameOrd),
                            ClientPhoneNumber = reader.GetString(clientPhoneNumberOrd),
                            TotalItems = reader.GetInt32(totalItemsOrd),
                            BookDate = reader.GetDateTime(bookDateOrd),
                            BookStart = reader.GetTimeSpan(bookStartOrd),
                            BookEnd = reader.GetTimeSpan(bookEndOrd)
                        });
                    }
                }
            }

            return orders;
        }

        // Get order lines for a specific order 
        public async Task<Order> GetOrderAsync(int orderId)
        {
            string query = @"
                SELECT o.order_id,
                       l.quantity, 
                       p.name AS product_name, p.image_url AS product_image,
                       c.name AS category_name,
                       cl.first_name AS client_firstname, cl.last_name AS client_lastname
                FROM dbo.Orders o
                LEFT JOIN dbo.Order_line l ON o.order_id = l.order_id
                LEFT JOIN dbo.Product p ON l.product_id = p.product_id
                LEFT JOIN dbo.Category c ON p.category_id = c.category_id
                LEFT JOIN dbo.Client cl ON o.user_id = cl.user_id
                WHERE o.order_id = @orderId";

            Order? order = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@orderId", orderId);
                await connection.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int orderIdOrd = reader.GetOrdinal("order_id");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int productNameOrd = reader.GetOrdinal("product_name");
                    int productImageOrd = reader.GetOrdinal("product_image");
                    int categoryNameOrd = reader.GetOrdinal("category_name");
                    int clientFirstnameOrd = reader.GetOrdinal("client_firstname");
                    int clientLastnameOrd = reader.GetOrdinal("client_lastname");

                    while (await reader.ReadAsync())
                    {
                        if (order == null)
                        {
                            order = new Order
                            {
                                Id = reader.GetInt32(orderIdOrd),
                                Client = new Client(reader.GetString(clientFirstnameOrd), reader.GetString(clientLastnameOrd))
                            };
                        }

                        Product p = new Product
                        {
                            Name = reader.GetString(productNameOrd),
                            ImageUrl = reader.GetString(productImageOrd),
                            Category = new Category(reader.GetString(categoryNameOrd))
                        };
                        OrderLine line = new OrderLine
                        {
                            Product = p,
                            Quantity = quantityOrd,
                        };

                        order.Lines.Add(line);
                    }
                }
            }

            return order;
        }
        
        public async Task<bool> UpdateCratesUsed(int orderId, int cratesCount, OrderStatus status)
        {
            bool success = false;

            string query = @"
                UPDATE dbo.Orders
                SET 
                    status=@status,
                    crates_used=@cratesCount
                WHERE order_id = @orderId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@status", status.ToString());
                cmd.Parameters.AddWithValue("@cratesCount", cratesCount);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                await connection.OpenAsync();

                int res = await cmd.ExecuteNonQueryAsync();
                success = res > 0;
            }

            return success;
        }
    }
}
