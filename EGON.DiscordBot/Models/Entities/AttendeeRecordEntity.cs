using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class AttendeeRecordEntity : ITableEntity
    {
        public string PartitionKey { get; set; }  // Event ID
        public string RowKey { get; set; }  // Unique id per attendee record
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string Role { get; set; }
        public string? Class { get; set; }
        public string? Spec { get; set; }
        public string? MinutesLate { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public AttendeeRecordEntity() { }

        public AttendeeRecordEntity(AttendeeRecord dto)
        {
            PartitionKey = dto.EventId.ToString();
            RowKey = dto.DiscordName;
            DiscordName = dto.DiscordName;
            DiscordDisplayName = dto.DiscordDisplayName;
            Role = dto.Role;
            Class = dto.Class;
            Spec = dto.Spec;
            MinutesLate = dto.MinutesLate;
        }

        public AttendeeRecord ToDto()
        {
            var attendeeRecord = new AttendeeRecord()
            {
                DiscordName = DiscordName,
                DiscordDisplayName = DiscordDisplayName,
                Role = Role,
                Class = Class,
                Spec = Spec,
                MinutesLate = MinutesLate,
                EventId = ulong.Parse(PartitionKey)
            };

            return attendeeRecord;
        }
    }
}
