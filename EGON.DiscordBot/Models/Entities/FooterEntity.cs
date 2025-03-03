using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class FooterEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string Value { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public FooterEntity() { }

        public FooterEntity(Footer dto)
        {
            PartitionKey = "Footer";
            RowKey = dto.Value;
            Value = dto.Value;
        }

        public Footer ToDto()
        {
            var dto = new Footer()
            {
                Value = Value
            };

            return dto;
        }
    }
}
