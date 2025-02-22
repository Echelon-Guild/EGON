namespace EGON.DiscordBot.Models
{
    public class ScheduledMessage
    {
        public ulong UserId { get; set; }
        public ulong EventId { get; set; }
        public string Message { get; set; }
        public string EventUrl { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
