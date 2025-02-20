using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class ScheduledMessageEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        
        public string Message { get; set; }
        public DateTimeOffset SendTime { get; set; }

        // These have to be a string for the Azure table storage query to work right. They should always parse to a ulong.
        public string EventId { get; set; }
        public string UserId { get; set; }
    }

}
