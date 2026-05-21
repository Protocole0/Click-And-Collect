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

        // Order (ViewModel) list for the employees (order picker and cashier)
        // This method retrieves all orders for a given status, store, and booking date
        public async Task<List<OrderDisplayViewModel>> GetAllOrdersAsync(OrderStatus status, int? storeId, DateTime bookDate)
        {
            var orderDictionary = new Dictionary<int, OrderDisplayViewModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        o.order_id, o.crates_used, o.status,
                        c.first_name, c.last_name, c.phone_number,
                        t.date_slot, t.start_time, t.end_time,
                        ol.quantity,
                        p.price
                    FROM dbo.orders o
                    JOIN dbo.client c ON o.client_id = c.client_id
                    JOIN dbo.time_slot t ON o.time_slot_id = t.time_slot_id
                    JOIN dbo.order_line ol ON o.order_id = ol.order_id
                    JOIN dbo.product p ON ol.product_id = p.product_id
                    WHERE o.status = @status 
                    AND o.store_id = @storeId
                    AND CAST(t.date_slot AS DATE) = CAST(@bookDate AS DATE)
                    ORDER BY t.start_time ASC, o.order_id ASC";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@status", Convert.ToString(status));
                cmd.Parameters.AddWithValue("@storeId", storeId);
                cmd.Parameters.AddWithValue("@bookDate", bookDate);

                await connection.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    // Here, all column indexes are retrieved
                    int orderIdOrd = reader.GetOrdinal("order_id");
                    int cratesUsedOrd = reader.GetOrdinal("crates_used");
                    int statusOrd = reader.GetOrdinal("status");
                    int clientFirstNameOrd = reader.GetOrdinal("first_name");
                    int clientLastNameOrd = reader.GetOrdinal("last_name");
                    int clientPhoneOrd = reader.GetOrdinal("phone_number");
                    int timeSlotDateOrd = reader.GetOrdinal("date_slot");
                    int timeSlotStartOrd = reader.GetOrdinal("start_time");
                    int timeSlotEndOrd = reader.GetOrdinal("end_time");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int productPriceOrd = reader.GetOrdinal("price");

                    while (await reader.ReadAsync())
                    {
                        int orderId = reader.GetInt32(orderIdOrd);

                        // This part must be executed only once per order,
                        // because the TryGetValue method checks by the orderId mentioned
                        // if the order mentioned already exists in the dictionary
                        if (!orderDictionary.TryGetValue(orderId, out OrderDisplayViewModel order))
                        {
                            order = new OrderDisplayViewModel
                            (
                                orderId,
                                reader.GetInt32(cratesUsedOrd),
                                (OrderStatus)Enum.Parse(typeof(OrderStatus), reader.GetString(statusOrd), true),
                                reader.GetString(clientFirstNameOrd),
                                reader.GetString(clientLastNameOrd),
                                reader.GetString(clientPhoneOrd),
                                reader.GetDateTime(timeSlotDateOrd),
                                reader.GetTimeSpan(timeSlotStartOrd),
                                reader.GetTimeSpan(timeSlotEndOrd)
                            );

                            // The order is added to the dictionary
                            orderDictionary.Add(orderId, order);
                        }

                        // For each order line, a new OrderLineDisplayViewModel
                        // is created and added to the Lines list of the corresponding order
                        // obtained by creating it with the out keyword in the TryGetValue method
                        order.Lines.Add(new OrderLineDisplayViewModel
                        (
                            reader.GetInt32(quantityOrd),
                            reader.GetDecimal(productPriceOrd)
                        ));
                    }
                }
            }

            return orderDictionary.Values.ToList();
        }

        // For the order picker
        public async Task<Order> GetOrderForChecklistAsync(int orderId)
        {
            string query = @"
                SELECT o.order_id,
                       l.quantity,
                       p.name AS product_name, p.image_url AS product_image,
                       c.name AS category_name,
                       cl.first_name AS client_firstname, cl.last_name AS client_lastname
                FROM dbo.Orders o
                LEFT JOIN dbo.Order_line l ON o.order_id = l.order_id
                LEFT JOIN dbo.Product p ON l.product_id  = p.product_id
                LEFT JOIN dbo.Category c ON p.category_id = c.category_id
                LEFT JOIN dbo.Client cl ON o.client_id   = cl.client_id
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
                        try
                        {
                            if (order == null)
                            {
                                order = new Order
                                (
                                    reader.GetInt32(orderIdOrd),
                                    new Client(reader.GetString(clientFirstnameOrd), reader.GetString(clientLastnameOrd))
                                );
                            }

                            Product p = new Product
                            {
                                Name = reader.GetString(productNameOrd),
                                ImageUrl = reader.GetString(productImageOrd),
                                Category = new Category(reader.GetString(categoryNameOrd))
                            };

                            order.Lines.Add(new OrderLine { Product = p, Quantity = reader.GetInt32(quantityOrd) });
                        }
                        catch (ArgumentException ex)
                        {
                            throw new InvalidOperationException(
                                $"Données invalides pour la commande id={reader.GetInt32(orderIdOrd)} : {ex.Message}", ex);
                        }
                    }
                }
            }

            return order!;
        }

        // For the cashier
        public async Task<Order> GetOrderForBillAsync(int orderId)
        {
            string query = @"
                SELECT o.order_id, o.crates_used,
                       l.quantity,
                       p.price AS product_price,
                       cl.first_name AS client_firstname, cl.last_name AS client_lastname, 
                       u.email AS client_email,
                       s.name AS store_name, s.street_name AS store_street, s.street_number AS store_number, s.city AS store_city, s.postal_code AS store_postal_code,
                       ts.date_slot, ts.start_time, ts.end_time
                FROM dbo.Orders o
                LEFT JOIN dbo.Order_line l ON o.order_id = l.order_id
                LEFT JOIN dbo.Product p ON l.product_id = p.product_id
                LEFT JOIN dbo.Client cl ON o.client_id = cl.client_id
                LEFT JOIN dbo.Users u ON cl.user_id = u.user_id
                LEFT JOIN dbo.Store s ON o.store_id = s.store_id
                LEFT JOIN dbo.Time_slot ts ON o.time_slot_id = ts.time_slot_id
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
                    int cratesUsedOrd = reader.GetOrdinal("crates_used");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int productPriceOrd = reader.GetOrdinal("product_price");
                    int clientFirstnameOrd = reader.GetOrdinal("client_firstname");
                    int clientLastnameOrd = reader.GetOrdinal("client_lastname");
                    int clientEmailOrd = reader.GetOrdinal("client_email");
                    int storeNameOrd = reader.GetOrdinal("store_name");
                    int storeStreetOrd = reader.GetOrdinal("store_street");
                    int storeNumberOrd = reader.GetOrdinal("store_number");
                    int storeCityOrd = reader.GetOrdinal("store_city");
                    int storePostalCodeOrd = reader.GetOrdinal("store_postal_code");
                    int dateSlotOrd = reader.GetOrdinal("date_slot");
                    int startTimeOrd = reader.GetOrdinal("start_time");
                    int endTimeOrd = reader.GetOrdinal("end_time");

                    while (await reader.ReadAsync())
                    {
                        if (order == null)
                        {
                            order = new Order
                            (
                                reader.GetInt32(orderIdOrd),
                                reader.GetInt32(cratesUsedOrd),
                                new Client(reader.GetString(clientFirstnameOrd), reader.GetString(clientLastnameOrd), reader.GetString(clientEmailOrd)),
                                new Store(reader.GetString(storeNameOrd), reader.GetString(storeStreetOrd), reader.GetString(storeNumberOrd), reader.GetString(storeCityOrd), reader.GetString(storePostalCodeOrd)),
                                new TimeSlot(reader.GetDateTime(dateSlotOrd), reader.GetTimeSpan(startTimeOrd), reader.GetTimeSpan(endTimeOrd))
                            );
                        }

                        // The Product object calls the constructor
                        // with the price only, because for the bill,
                        // only the price is needed
                        order.Lines.Add(new OrderLine(new Product(reader.GetDecimal(productPriceOrd)), reader.GetInt32(quantityOrd)));
                    }
                }
            }

            return order!;
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
                  JOIN store s ON o.store_id = s.store_id
                  JOIN time_slot ts ON o.time_slot_id = ts.time_slot_id
                  LEFT JOIN order_line ol ON ol.order_id = o.order_id
                  LEFT JOIN product p ON ol.product_id = p.product_id
                  WHERE o.client_id = @clientId
                  ORDER BY o.order_date DESC",
                conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            int ordIdOrd = reader.GetOrdinal("order_id");
            int dateOrd = reader.GetOrdinal("order_date");
            int crUsedOrd = reader.GetOrdinal("crates_used");
            int crRetOrd = reader.GetOrdinal("crates_returned");
            int statusOrd = reader.GetOrdinal("status");
            int storeIdOrd = reader.GetOrdinal("store_id");
            int stNameOrd = reader.GetOrdinal("store_name");
            int stStreetOrd = reader.GetOrdinal("street_name");
            int stNumOrd = reader.GetOrdinal("street_number");
            int stCityOrd = reader.GetOrdinal("city");
            int stZipOrd = reader.GetOrdinal("postal_code");
            int tsIdOrd = reader.GetOrdinal("time_slot_id");
            int tsDateOrd = reader.GetOrdinal("date_slot");
            int tsStartOrd = reader.GetOrdinal("start_time");
            int tsEndOrd = reader.GetOrdinal("end_time");
            int olIdOrd = reader.GetOrdinal("order_line_id");
            int olQtyOrd = reader.GetOrdinal("quantity");
            int pIdOrd = reader.GetOrdinal("product_id");
            int pNameOrd = reader.GetOrdinal("product_name");
            int pImgOrd = reader.GetOrdinal("image_url");
            int pPriceOrd = reader.GetOrdinal("price");

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
                    try
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
                    catch (ArgumentException ex)
                    {
                        throw new InvalidOperationException(
                            $"Données invalides pour une ligne de la commande id={orderId} : {ex.Message}", ex);
                    }
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
                cmdOrder.Parameters.AddWithValue("@date", order.OrderDate);
                cmdOrder.Parameters.AddWithValue("@crates_used", order.CratesUsed);
                cmdOrder.Parameters.AddWithValue("@crates_returned", order.CratesReturned);
                cmdOrder.Parameters.AddWithValue("@status", order.Status.ToString());
                cmdOrder.Parameters.AddWithValue("@clientId", order.Client!.Id);
                cmdOrder.Parameters.AddWithValue("@timeSlotId", order.Slot!.TimeSlotId);
                cmdOrder.Parameters.AddWithValue("@storeId", order.Store!.StoreId);

                int orderId = (int)(await cmdOrder.ExecuteScalarAsync())!;

                foreach (OrderLine line in order.Lines)
                {
                    SqlCommand cmdLine = new SqlCommand(
                        "INSERT INTO order_line (quantity, unit_price, product_id, order_id) VALUES (@qty, @unitPrice, @productId, @orderId)",
                        conn, tx);
                    cmdLine.Parameters.AddWithValue("@qty", line.Quantity);
                    cmdLine.Parameters.AddWithValue("@unitPrice", line.Product.Price);
                    cmdLine.Parameters.AddWithValue("@productId", line.Product.ProductId);
                    cmdLine.Parameters.AddWithValue("@orderId", orderId);
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

        public async Task<bool> UpdateCratesUsed(int orderId, int cratesCount)
        {
            string query = @"
                UPDATE dbo.Orders
                SET status = @status,
                    crates_used = @cratesCount
                WHERE order_id = @orderId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@status", OrderStatus.READY_FOR_PICKUP.ToString());
            cmd.Parameters.AddWithValue("@cratesCount", cratesCount);
            cmd.Parameters.AddWithValue("@orderId", orderId);
            await connection.OpenAsync();
            int res = await cmd.ExecuteNonQueryAsync();
            return res > 0;
        }

        public async Task<bool> UpdateCratesReturned(int orderId, int cratesCount)
        {
            string query = @"
                UPDATE dbo.Orders
                SET status = @status,
                    crates_returned = @cratesCount
                WHERE order_id = @orderId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@status", OrderStatus.COLLECTED.ToString());
            cmd.Parameters.AddWithValue("@cratesCount", cratesCount);
            cmd.Parameters.AddWithValue("@orderId", orderId);
            await connection.OpenAsync();
            int res = await cmd.ExecuteNonQueryAsync();
            return res > 0;
        }
    }
}
