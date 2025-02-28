namespace EGON.DiscordBot.Models
{
    public class ScheduledPost
    {
        public ulong ChannelId { get; set; }
        public ulong EventId { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
