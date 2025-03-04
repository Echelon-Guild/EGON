using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.DiscordBot.Models.Entities
{
    public class WoWEventLogEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public ulong EventId { get; set; }
        public string LogUrl { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public WoWEventLogEntity() { }

        public WoWEventLogEntity(WoWEventLog dto)
        {
            PartitionKey = "Logs";
            RowKey = dto.EventId.ToString();

            EventId = dto.EventId;
            LogUrl = dto.LogUrl;
        }

        public WoWEventLog ToDto()
        {
            var dto = new WoWEventLog()
            {
                EventId = EventId,
                LogUrl = LogUrl
            };

            return dto;
        }
    }
}
