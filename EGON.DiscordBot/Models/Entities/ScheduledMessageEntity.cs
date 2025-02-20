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

        public ulong UserId { get; set; }
        public string EventId { get; set; }
        public string Message { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }

}
