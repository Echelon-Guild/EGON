using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class WoWCharacterEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public Guid Id { get; set; }
        public string CharacterName { get; set; }
        public string CharacterRealm { get; set; }
        public string RegisteredTo { get; set; }
        public string Class { get; set; }
        public string Specialization { get; set; }
        public string OffSpec { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
