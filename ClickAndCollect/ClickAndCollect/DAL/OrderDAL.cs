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
                   c.client_id, c.first_name, c.last_name, c.phone_number
                FROM dbo.Orders o
                JOIN dbo.Client c ON o.client_id = c.client_id
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

        public async Task<List<Order>> GetOrdersByClientAsync(int clientId)
        {
            var dict = new Dictionary<int, Order>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                @"SELECT o.order_id, o.order_date, o.crates_used, o.crates_returned, o.status,
                         s.store_id, s.name AS store_name, s.street_name, s.street_number, s.city, s.postal_code,
                         ts.time_slot_id, ts.date_slot, ts.start_time, ts.end_time,
                         ol.order_line_id, ol.quantity, ol.unit_price,
                         p.product_id, p.name AS product_name, p.image_url, p.price
                  FROM orders o
                  JOIN store s         ON o.store_id      = s.store_id
                  JOIN time_slot ts    ON o.time_slot_id  = ts.time_slot_id
                  LEFT JOIN order_line ol ON ol.order_id  = o.order_id
                  LEFT JOIN product p     ON ol.product_id = p.product_id
                  WHERE o.client_id = @clientId
                  ORDER BY o.order_date DESC",
                conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            int ordIdOrd    = reader.GetOrdinal("order_id");
            int dateOrd     = reader.GetOrdinal("order_date");
            int crUsedOrd   = reader.GetOrdinal("crates_used");
            int crRetOrd    = reader.GetOrdinal("crates_returned");
            int statusOrd   = reader.GetOrdinal("status");
            int storeIdOrd  = reader.GetOrdinal("store_id");
            int stNameOrd   = reader.GetOrdinal("store_name");
            int stStreetOrd = reader.GetOrdinal("street_name");
            int stNumOrd    = reader.GetOrdinal("street_number");
            int stCityOrd   = reader.GetOrdinal("city");
            int stZipOrd    = reader.GetOrdinal("postal_code");
            int tsIdOrd     = reader.GetOrdinal("time_slot_id");
            int tsDateOrd   = reader.GetOrdinal("date_slot");
            int tsStartOrd  = reader.GetOrdinal("start_time");
            int tsEndOrd    = reader.GetOrdinal("end_time");
            int olIdOrd     = reader.GetOrdinal("order_line_id");
            int olQtyOrd    = reader.GetOrdinal("quantity");
            int olPriceOrd  = reader.GetOrdinal("unit_price");
            int pIdOrd      = reader.GetOrdinal("product_id");
            int pNameOrd    = reader.GetOrdinal("product_name");
            int pImgOrd     = reader.GetOrdinal("image_url");
            int pPriceOrd   = reader.GetOrdinal("price");

            while (await reader.ReadAsync())
            {
                int orderId = reader.GetInt32(ordIdOrd);

                if (!dict.TryGetValue(orderId, out Order? order))
                {
                    var store = new Store(
                        reader.GetInt32(storeIdOrd),
                        reader.GetString(stNameOrd),
                        reader.GetString(stStreetOrd),
                        reader.GetString(stNumOrd),
                        reader.GetString(stCityOrd),
                        reader.GetString(stZipOrd));

                    var slot = new TimeSlot(
                        reader.GetInt32(tsIdOrd),
                        reader.GetDateTime(tsDateOrd),
                        reader.GetTimeSpan(tsStartOrd),
                        reader.GetTimeSpan(tsEndOrd));

                    order = new Order(
                        orderId,
                        reader.GetDateTime(dateOrd),
                        reader.GetInt32(crUsedOrd),
                        reader.GetInt32(crRetOrd),
                        (OrderStatus)Enum.Parse(typeof(OrderStatus), reader.GetString(statusOrd)),
                        null!,
                        new List<OrderLine>(),
                        store,
                        slot);

                    dict[orderId] = order;
                }

                if (!reader.IsDBNull(olIdOrd))
                {
                    var product = new Product(
                        reader.GetInt32(pIdOrd),
                        reader.GetString(pNameOrd),
                        null,
                        reader.GetDecimal(pPriceOrd),
                        reader.GetString(pImgOrd),
                        null,
                        null);

                    order.Lines.Add(new OrderLine(
                        reader.GetInt32(olIdOrd),
                        reader.GetInt32(olQtyOrd),
                        product));
                }
            }

            return dict.Values.ToList();
        }

        public async Task CreateAsync(Order order)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using SqlTransaction tx = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                SqlCommand cmdOrder = new SqlCommand(
                    "INSERT INTO orders (order_date, crates_used, crates_returned, status, client_id, time_slot_id, store_id) " +
                    "OUTPUT INSERTED.order_id " +
                    "VALUES (@date, @crates_used, @crates_returned, @status, @clientId, @timeSlotId, @storeId)",
                    conn, tx);
                cmdOrder.Parameters.AddWithValue("@date",            order.OrderDate);
                cmdOrder.Parameters.AddWithValue("@crates_used",     order.CratesUsed);
                cmdOrder.Parameters.AddWithValue("@crates_returned", order.CratesReturned);
                cmdOrder.Parameters.AddWithValue("@status",          order.Status.ToString());
                cmdOrder.Parameters.AddWithValue("@clientId",        order.Client!.Id);
                cmdOrder.Parameters.AddWithValue("@timeSlotId",     order.Slot!.TimeSlotId);
                cmdOrder.Parameters.AddWithValue("@storeId",        order.Store!.StoreId);

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
