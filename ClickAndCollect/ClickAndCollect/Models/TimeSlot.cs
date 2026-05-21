using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Models
{
    public class TimeSlot
    {
        private int _timeSlotId;
        public int TimeSlotId
        {
            get => _timeSlotId;
            set
            {
                if (value < 0)
                    throw new ArgumentException("L'id d'un créneau horaire ne peut être négatif.");
                _timeSlotId = value;
            }
        }

        private DateTime _dateSlot;
        public DateTime DateSlot
        {
            get => _dateSlot;
            set
            {
                if (value.Date < DateTime.Today)
                    throw new ArgumentException("Une date de réservation ne peut pas être instanciée à un jour précédent aujourd'hui.");
                _dateSlot = value;
            }
        }

        private TimeSpan _startTime;
        public TimeSpan StartTime
        {
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
            _dateSlot = dateSlot;
            _startTime = startTime;
            EndTime = endTime;
        }

        public TimeSlot(DateTime dateSlot, TimeSpan startTime, TimeSpan endTime)
        {
            _dateSlot = dateSlot;
            _startTime = startTime;
            _endTime = endTime;
        }

        // --- Static methods : the class delegates to the DAL ---

        private const int MaxReservationsPerSlot = 10;

        public bool IsAvailable => Reservations < MaxReservationsPerSlot;

        public int PlacesLeft => MaxReservationsPerSlot - Reservations;

        // --- Static methods : the class delegates to the DAL then applies business rules ---

        public static async Task<TimeSlot?> GetById(int timeSlotId, ITimeSlotDAL timeSlotDAL)
        {
            return await timeSlotDAL.GetByIdAsync(timeSlotId);
        }

        // All available slots for a store (rule: less than 10 reservations)
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
