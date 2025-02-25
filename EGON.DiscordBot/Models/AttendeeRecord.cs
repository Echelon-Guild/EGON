using System.ComponentModel.DataAnnotations;

namespace EGON.DiscordBot.Models
{
    public class AttendeeRecord
    {
        public int Id { get; set; }
        public ulong EventId { get; set; }
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string Role { get; set; }
        public string? Class { get; set; }
        public string? Spec { get; set; }
        public string? MinutesLate { get; set; }
    }
}
