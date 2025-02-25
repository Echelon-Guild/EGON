namespace EGON.DiscordBot.Models
{
    public class EchelonEvent
    {
        public ulong Id { get; set; }
        public ulong MessageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Organizer { get; set; }
        public string OrganizerUserId { get; set; }
        public string ImageUrl { get; set; }
        public string Footer { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public EventType EventType { get; set; }
        public string MessageUrl { get; set; }
    }

    public enum EventType
    {
        Raid,
        Dungeon,
        Event
    }
}
