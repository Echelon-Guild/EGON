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

        public WoWCharacterEntity() { }

        public WoWCharacterEntity(WoWCharacter dto)
        {
            PartitionKey = dto.Class;
            RowKey = dto.Id.ToString();

            Id = dto.Id;
            CharacterName = dto.CharacterName;
            CharacterRealm = dto.CharacterRealm;
            RegisteredTo = dto.RegisteredTo;
            Class = dto.Class;
            Specialization = dto.Specialization;
            OffSpec = dto.OffSpec;
        }

        public WoWCharacter ToDto()
        {
            var character = new WoWCharacter()
            {
                Id = Id,
                CharacterName = CharacterName,
                CharacterRealm = CharacterRealm,
                RegisteredTo = RegisteredTo,
                Class = Class,
                Specialization = Specialization,
                OffSpec = OffSpec
            };

            return character;

        }
    }
}
