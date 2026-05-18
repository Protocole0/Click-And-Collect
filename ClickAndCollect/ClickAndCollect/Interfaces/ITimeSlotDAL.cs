using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface ITimeSlotDAL
    {
        Task<TimeSlot?> GetByIdAsync(int timeSlotId);
        Task<List<TimeSlot>> GetFutureWithCountsAsync(int storeId);
    }
}
