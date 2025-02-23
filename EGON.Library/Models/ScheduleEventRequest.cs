namespace EGON.Library.Models
{
    public class ScheduleEventRequest
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventType? EventType { get; set; }
        public int? Year { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string ImageUrl { get; set; }

        public TimeSpan Offset { get; set; }
    }
}
