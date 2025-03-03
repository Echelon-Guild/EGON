using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class WoWInstanceInfoEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool Legacy { get; set; }
        public InstanceType InstanceType { get; set; }

        public ETag ETag { get; set; }

        public WoWInstanceInfoEntity() { }

        public WoWInstanceInfoEntity(WoWInstanceInfo dto)
        {
            PartitionKey = dto.InstanceType.ToString();
            RowKey = dto.Name;

            Name = dto.Name;
            ImageUrl = dto.ImageUrl;
            Legacy = dto.Legacy;
            InstanceType = dto.InstanceType;
        }

        public WoWInstanceInfo ToDto()
        {
            var instance = new WoWInstanceInfo()
            {
                Name = Name,
                ImageUrl = ImageUrl,
                Legacy = Legacy,
                InstanceType = InstanceType
            };

            return instance;
        }
    }
}