using Azure;
using Azure.Data.Tables;

namespace EGON.Blazor.Models.Entities
{
    public class EchelonEventEntity : ITableEntity
    {
        public string PartitionKey { get; set; }  // E.g., "Raid", "Mythic", "Meeting"
        public string RowKey { get; set; }  // Unique event ID
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public string Organizer { get; set; }
        public string OrganizerUserId { get; set; }
        public string ImageUrl { get; set; }
        public string Footer { get; set; }
        public ulong MessageId { get; set; }
        public string MessageUrl { get; set; }

        // This HAS to be a string for the query, a ulong fails to convert properly
        public string EventId { get; set; }


        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }

}
