using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class EGONSettingEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public EGONSettingEntity() { }

        public EGONSettingEntity(EGONSetting dto)
        {
            PartitionKey = "Settings";
            RowKey = dto.Name;

            Name = dto.Name;
            Value = dto.Value;
        }

        public EGONSetting ToDto()
        {
            var dto = new EGONSetting()
            {
                Name = Name,
                Value = Value
            };

            return dto;
        }
    }
}
