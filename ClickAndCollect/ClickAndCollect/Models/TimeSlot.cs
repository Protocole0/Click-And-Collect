using ClickAndCollect.Interfaces;
using System.Globalization;

namespace ClickAndCollect.Models
{
    public class TimeSlot
    {
        private int _timeSlotId;
        public int TimeSlotId { 
            get => _timeSlotId; 
            set => _timeSlotId = value; 
        }

        private DateTime _dateSlot;
        public DateTime DateSlot { 
            get => _dateSlot; 
            set => _dateSlot = value; 
        }

        private TimeSpan _startTime;
        public TimeSpan StartTime { 
            get => _startTime; 
            set => _startTime = value; 
        }

        private TimeSpan _endTime;
        public TimeSpan EndTime
        {
            get => _endTime;
            set => _endTime = value > _startTime
                ? value
                : throw new ArgumentException("L'heure de fin doit être supérieure à l'heure de début.");
        }

        public int Reservations { get; set; }

        public TimeSlot() { }

        public TimeSlot(int timeSlotId, DateTime dateSlot, TimeSpan startTime, TimeSpan endTime)
        {
            _timeSlotId = timeSlotId;
            _dateSlot   = dateSlot;
            _startTime  = startTime;
            EndTime     = endTime;
        }

        // --- Méthodes statiques : la classe délègue au DAL ---

        private const int MaxReservationsPerSlot = 10;

        public bool IsAvailable => Reservations < MaxReservationsPerSlot;

        public int PlacesLeft   => MaxReservationsPerSlot - Reservations;

        // --- Méthodes statiques : la classe délègue au DAL puis applique les règles métier ---

        public static async Task<TimeSlot?> GetById(int timeSlotId, ITimeSlotDAL timeSlotDAL)
        {
            return await timeSlotDAL.GetByIdAsync(timeSlotId);
        }

        // Tous les créneaux disponibles d'un magasin (règle : < 10 réservations)
        public static async Task<List<TimeSlot>> GetAvailableAsync(int storeId, ITimeSlotDAL timeSlotDAL)
        {
            List<TimeSlot> all = await timeSlotDAL.GetFutureWithCountsAsync(storeId);

            return all
                .Where(s => s.IsAvailable)
                .OrderBy(s => s._dateSlot)
                .ThenBy(s => s._startTime)
                .ToList();
        }
    }
}
