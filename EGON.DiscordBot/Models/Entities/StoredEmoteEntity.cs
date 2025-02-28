using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class StoredEmoteEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public string ClassName { get; set; }
        public string SpecName { get; set; }
        public string EmoteID { get; set; }

        public StoredEmoteEntity() { }

        public StoredEmoteEntity(StoredEmote dto)
        {
            PartitionKey = dto.ClassName;
            RowKey = dto.SpecName;

            ClassName = dto.ClassName;
            SpecName = dto.SpecName;
            EmoteID = dto.EmoteID;
        }

        public StoredEmote ToDTO()
        {
            var emote = new StoredEmote()
            {
                ClassName = ClassName,
                SpecName = SpecName,
                EmoteID = EmoteID
            };

            return emote;
        }
    }
}
