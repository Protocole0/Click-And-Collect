namespace ClickAndCollect.Models
{
    public class TimeSlot
    {
		private int _id;

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private DateTime _date;

		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}

		private TimeOnly _startTime;

		public TimeOnly StartTime
		{
			get { return _startTime; }
			set { _startTime = value; }
		}

		private TimeOnly _endTime;

		public TimeOnly EndTime
		{
			get { return _endTime; }
			set { _endTime = value; }
		}

	}
}
