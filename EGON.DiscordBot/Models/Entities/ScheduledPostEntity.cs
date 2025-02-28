using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class ScheduledPostEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public ulong ChannelId { get; set; }
        public ulong EventId { get; set; }
        public DateTimeOffset SendTime { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ScheduledPostEntity() { }

        public ScheduledPostEntity(ScheduledPost dto)
        {
            PartitionKey = "ScheduledPost";
            RowKey = dto.EventId.ToString();

            ChannelId = dto.ChannelId;
            EventId = dto.EventId;
            SendTime = dto.SendTime;
        }

        public ScheduledPost ToDTO()
        {
            var post = new ScheduledPost()
            {
                ChannelId = ChannelId,
                EventId = EventId,
                SendTime = SendTime
            };

            return post;
        }
    }
}
