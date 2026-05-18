using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
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
        public async Task<List<Order>> GetAllOrdersAsync(OrderStatus status)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = 
                @"SELECT 
                   o.order_id, o.order_date, o.crates_used, o.crates_returned, o.status,
                   c.user_id AS client_id, c.first_name, c.last_name, c.phone_number
                FROM dbo.Orders o
                JOIN dbo.Client c ON o.user_id = c.user_id
                WHERE o.status = @status";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("Status", Convert.ToString(status));
                await connection.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int orderIdOrd = reader.GetOrdinal("order_id");
                    int orderDateOrd = reader.GetOrdinal("order_date");
                    int cratesUsedOrd = reader.GetOrdinal("crates_used");
                    int cratesReturnedOrd = reader.GetOrdinal("crates_returned");
                    int statusOrd = reader.GetOrdinal("status");
                    int clientIdOrd = reader.GetOrdinal("client_id");
                    int clientFirstnameOrd = reader.GetOrdinal("first_name");
                    int clientLastnameOrd = reader.GetOrdinal("last_name");
                    int clientPhoneNumberOrd = reader.GetOrdinal("phone_number");

                    while (await reader.ReadAsync())
                    {
                        Client orderClient = new Client(
                            reader.GetInt32(clientIdOrd),
                            reader.GetString(clientFirstnameOrd),
                            reader.GetString(clientLastnameOrd),
                            reader.GetString(clientPhoneNumberOrd)
                            );
                        orders.Add(new Order(
                            reader.GetInt32(orderIdOrd),
                            reader.GetDateTime(orderDateOrd),
                            reader.GetInt32(cratesUsedOrd),
                            reader.GetInt32(cratesReturnedOrd),
                            (OrderStatus)Enum.Parse(typeof(OrderStatus), reader.GetString(statusOrd)),
                            orderClient
                            ));
                    }
                }
            }

            return orders;
        }

        // Get order lines for a specific order 
        public async Task<Order> GetOrderAsync(int orderId)
        {
            Order order = new Order(orderId);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query =
                @"SELECT 
                   ol.order_line_id, ol.quantity,
                   p.name AS product_name, p.image_url AS product_image,
                   c.name AS category_name
                FROM dbo.Order_Line ol
                JOIN dbo.Product p ON ol.product_id = p.product_id
                JOIN dbo.Category c ON p.category_id = c.category_id
                WHERE ol.order_id = @orderId";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("orderId", orderId);
                await connection.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    int orderLineIdOrd = reader.GetOrdinal("order_line_id");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int productNameOrd = reader.GetOrdinal("product_name");
                    int productImageOrd = reader.GetOrdinal("product_image");
                    int categoryNameOrd = reader.GetOrdinal("category_name");

                    while (await reader.ReadAsync())
                    {
                        Category productCategory = new Category(reader.GetString(categoryNameOrd));
                        Product product = new Product(reader.GetString(productNameOrd), reader.GetString(productImageOrd), productCategory);
                        OrderLine orderLine = new OrderLine(
                            reader.GetInt32(orderLineIdOrd),
                            reader.GetInt32(quantityOrd),
                            product
                            );
                        order.Lines.Add(orderLine);
                    }
                }
            }

            return order;
        }

        public async Task CreateAsync(Order order)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using SqlTransaction tx = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                SqlCommand cmdOrder = new SqlCommand(
                    "INSERT INTO orders (order_date, crates_used, crates_returned, status, user_id, time_slot_id, store_id) " +
                    "OUTPUT INSERTED.order_id " +
                    "VALUES (@date, @crates_used, @crates_returned, @status, @userId, @timeSlotId, @storeId)",
                    conn, tx);
                cmdOrder.Parameters.AddWithValue("@date",           order.OrderDate);
                cmdOrder.Parameters.AddWithValue("@crates_used",    order.CratesUsed);
                cmdOrder.Parameters.AddWithValue("@crates_returned", order.CratesReturned);
                cmdOrder.Parameters.AddWithValue("@status",         order.Status.ToString());
                cmdOrder.Parameters.AddWithValue("@userId",         order.Client!.Id);
                cmdOrder.Parameters.AddWithValue("@timeSlotId",     order.TimeSlotId);
                cmdOrder.Parameters.AddWithValue("@storeId",        order.StoreId);

                int orderId = (int)(await cmdOrder.ExecuteScalarAsync())!;

                foreach (OrderLine line in order.Lines)
                {
                    SqlCommand cmdLine = new SqlCommand(
                        "INSERT INTO order_line (quantity, unit_price, product_id, order_id) VALUES (@qty, @unitPrice, @productId, @orderId)",
                        conn, tx);
                    cmdLine.Parameters.AddWithValue("@qty",       line.Quantity);
                    cmdLine.Parameters.AddWithValue("@unitPrice", line.Product.Price);
                    cmdLine.Parameters.AddWithValue("@productId", line.Product.ProductId);
                    cmdLine.Parameters.AddWithValue("@orderId",   orderId);
                    await cmdLine.ExecuteNonQueryAsync();
                }

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
