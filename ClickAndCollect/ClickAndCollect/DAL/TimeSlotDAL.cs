using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class TimeSlotDAL : ITimeSlotDAL
    {
        private readonly string _connectionString;

        public TimeSlotDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TimeSlot?> GetByIdAsync(int timeSlotId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                "SELECT time_slot_id, date_slot, start_time, end_time FROM time_slot WHERE time_slot_id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", timeSlotId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return new TimeSlot(
                    reader.GetInt32(reader.GetOrdinal("time_slot_id")),
                    reader.GetDateTime(reader.GetOrdinal("date_slot")),
                    reader.GetTimeSpan(reader.GetOrdinal("start_time")),
                    reader.GetTimeSpan(reader.GetOrdinal("end_time")));
            return null;
        }

        // Retourne tous les créneaux futurs avec le nombre de réservations pour ce magasin.
        // Le filtrage métier (< 10) est délégué à la classe TimeSlot.
        public async Task<List<TimeSlot>> GetFutureWithCountsAsync(int storeId)
        {
            List<TimeSlot> slots = new List<TimeSlot>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(
                @"SELECT ts.time_slot_id, ts.date_slot, ts.start_time, ts.end_time,
                         (SELECT COUNT(*) FROM orders o
                          WHERE o.time_slot_id = ts.time_slot_id
                            AND o.store_id = @storeId) AS reservations
                  FROM time_slot ts
                  WHERE ts.date_slot > CAST(GETDATE() AS DATE)
                  ORDER BY ts.date_slot, ts.start_time",
                conn);
            cmd.Parameters.AddWithValue("@storeId", storeId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            int idOrd    = reader.GetOrdinal("time_slot_id");
            int dateOrd  = reader.GetOrdinal("date_slot");
            int startOrd = reader.GetOrdinal("start_time");
            int endOrd   = reader.GetOrdinal("end_time");
            int resOrd   = reader.GetOrdinal("reservations");

            while (await reader.ReadAsync())
            {
                try
                {
                    var slot = new TimeSlot(
                        reader.GetInt32(idOrd),
                        reader.GetDateTime(dateOrd),
                        reader.GetTimeSpan(startOrd),
                        reader.GetTimeSpan(endOrd));
                    slot.Reservations = reader.GetInt32(resOrd);
                    slots.Add(slot);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException(
                        $"Données invalides pour le créneau id={reader.GetInt32(idOrd)} : {ex.Message}", ex);
                }
            }

            return slots;
        }
    }
}
