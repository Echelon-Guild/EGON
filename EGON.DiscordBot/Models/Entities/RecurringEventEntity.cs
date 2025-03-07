using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class RecurringEventEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string EventName { get; set; }
        public DateTimeOffset FirstEventDateTime { get; set; }
        

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
