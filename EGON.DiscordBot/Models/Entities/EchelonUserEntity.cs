using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class EchelonUserEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Users";
        public string RowKey { get; set; }
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string TimeZone { get; set; }
        public string Class { get; set; }
        public string Spec { get; set; }


        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public EchelonUserEntity() { }

        public EchelonUserEntity(EchelonUser dto)
        {
            RowKey = dto.DiscordName;
            DiscordName = dto.DiscordName;
            DiscordDisplayName = dto.DiscordDisplayName;
            TimeZone = dto.TimeZone;
            Class = dto.Class;
            Spec = dto.Spec;
        }

        public EchelonUser ToDto()
        {
            var user = new EchelonUser()
            {
                DiscordName = DiscordName,
                DiscordDisplayName = DiscordDisplayName,
                TimeZone = TimeZone,
                Class = Class,
                Spec = Spec
            };

            return user;
        }
    }
}
