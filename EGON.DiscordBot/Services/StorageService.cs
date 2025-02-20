using Azure.Data.Tables;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Models.Entities;

namespace EGON.DiscordBot.Services
{
    public class StorageService
    {
        private readonly TableClient _eventTable;
        private readonly TableClient _attendeeRecordTable;
        private readonly TableClient _echelonUserTable;
        private readonly TableClient _scheduledMessageTable;
        private readonly TableClient _storedEmoteTable;
        private readonly TableClient _wowCharacterTable;
        private readonly TableClient _wowInstanceInfoTable;
        private readonly TableClient _wowTeamTable;

        public StorageService(TableServiceClient tableServiceClient)
        {
            _eventTable = tableServiceClient.GetTableClient(TableNames.EVENT_TABLE_NAME);
            _eventTable.CreateIfNotExists();

            _attendeeRecordTable = tableServiceClient.GetTableClient(TableNames.ATTENDEE_TABLE_NAME);
            _attendeeRecordTable.CreateIfNotExists();

            _echelonUserTable = tableServiceClient.GetTableClient(TableNames.USER_TABLE_NAME);
            _echelonUserTable.CreateIfNotExists();

            _scheduledMessageTable = tableServiceClient.GetTableClient(TableNames.SCHEDULED_MESSAGE_TABLE_NAME);
            _scheduledMessageTable.CreateIfNotExists();

            _storedEmoteTable = tableServiceClient.GetTableClient(TableNames.EMOTE_TABLE_NAME);
            _storedEmoteTable.CreateIfNotExists();

            _wowCharacterTable = tableServiceClient.GetTableClient(TableNames.WOW_CHARACTER_TABLE_NAME);
            _wowCharacterTable.CreateIfNotExists();

            _wowInstanceInfoTable = tableServiceClient.GetTableClient(TableNames.INSTANCE_TABLE_NAME);
            _wowInstanceInfoTable.CreateIfNotExists();

            _wowTeamTable = tableServiceClient.GetTableClient(TableNames.TEAM_TABLE_NAME);
            _wowTeamTable.CreateIfNotExists();
        }

        // Events
        public async Task UpsertEventAsync(EchelonEvent ecEvent)
        {
            var entity = new EchelonEventEntity()
            {
                EventDateTime = ecEvent.EventDateTime,
                EventDescription = ecEvent.Description,
                EventName = ecEvent.Name,
                Footer = ecEvent.Footer,
                ImageUrl = ecEvent.ImageUrl,
                MessageId = ecEvent.MessageId,
                Organizer = ecEvent.Organizer,
                PartitionKey = ecEvent.EventType.ToString(),
                RowKey = ecEvent.MessageId.ToString()
            };

            await _eventTable.UpsertEntityAsync(entity);
        }

        public EchelonEvent? GetEvent(ulong messageId)
        {
            EchelonEventEntity? entity = _eventTable.Query<EchelonEventEntity>(e => e.RowKey == messageId.ToString()).FirstOrDefault();

            if (entity is null) { return null; }

            var echelonEvent = new EchelonEvent()
            {
                EventDateTime = entity.EventDateTime,
                Description = entity.EventDescription,
                Name = entity.EventName,
                Footer = entity.Footer,
                ImageUrl = entity.ImageUrl,
                MessageId = entity.MessageId,
                Organizer = entity.Organizer,
                EventType = Enum.Parse<EventType>(entity.PartitionKey)
            };

            return echelonEvent;

        }

        public async Task DeleteEventAsync(EchelonEvent ecEvent)
        {
            EchelonEventEntity? entity = _eventTable.Query<EchelonEventEntity>(e => e.RowKey == ecEvent.MessageId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _eventTable.DeleteEntityAsync(entity);
        }

        // Attendees

        public async Task UpsertAttendeesAsync(IEnumerable<AttendeeRecord> attendeeRecords)
        {
            List<Task> tasks = new();

            foreach (AttendeeRecord record in attendeeRecords)
            {
                var entity = new AttendeeRecordEntity
                {
                    PartitionKey = record.EventId.ToString(),
                    RowKey = record.DiscordName,
                    DiscordName = record.DiscordName,
                    DiscordDisplayName = record.DiscordDisplayName,
                    Role = record.Role,
                    Class = record.Class,
                    Spec = record.Spec
                };

                tasks.Add(_attendeeRecordTable.UpsertEntityAsync(entity));
            }

            await Task.WhenAll(tasks);
        }

        public IEnumerable<AttendeeRecord>? GetAttendeeRecords(ulong messageId)
        {
            IEnumerable<AttendeeRecordEntity> records = _attendeeRecordTable.Query<AttendeeRecordEntity>(e => e.PartitionKey == messageId.ToString());

            foreach (AttendeeRecordEntity record in records)
            {
                var attendeeRecord = new AttendeeRecord()
                {
                    EventId = int.Parse(record.PartitionKey),
                    DiscordName = record.DiscordName,
                    DiscordDisplayName = record.DiscordDisplayName,
                    Role = record.Role,
                    Class = record.Class,
                    Spec = record.Spec
                };

                yield return attendeeRecord;
            }
        }

        public async Task DeleteAttendeeRecordAsync(AttendeeRecord record)
        {
            AttendeeRecordEntity? entity = _attendeeRecordTable.Query<AttendeeRecordEntity>(e => e.RowKey == record.DiscordName && e.PartitionKey == record.EventId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _attendeeRecordTable.DeleteEntityAsync(entity);
        }

        // Users

        public async Task UpsertUserAsync(EchelonUser user)
        {
            EchelonUserEntity entity = new()
            {
                PartitionKey = "Users",
                RowKey = user.DiscordName,

                Class = user.Class,
                DiscordDisplayName = user.DiscordDisplayName,
                DiscordName = user.DiscordName,
                Spec = user.Spec,
                TimeZone = user.TimeZone
            };

            await _echelonUserTable.UpsertEntityAsync(entity);
        }

        public EchelonUser? GetUser(string discordUserName)
        {
            EchelonUserEntity? entity = _echelonUserTable.Query<EchelonUserEntity>(e => e.RowKey == discordUserName).FirstOrDefault();

            if (entity is null) { return null; }

            EchelonUser user = new()
            {
                Class = entity.Class,
                DiscordDisplayName = entity.DiscordDisplayName,
                DiscordName = entity.DiscordName,
                Spec = entity.Spec,
                TimeZone = entity.TimeZone
            };

            return user;
        }

        public async Task DeleteUserAsync(EchelonUser user)
        {
            EchelonUserEntity? entity = _echelonUserTable.Query<EchelonUserEntity>(e => e.RowKey == user.DiscordName).FirstOrDefault();

            if (entity is null) { return; }

            await _echelonUserTable.DeleteEntityAsync(entity);
        }

        public bool IsUserRegisteredToCreateEvents(string discordUserName)
        {
            EchelonUser? user = GetUser(discordUserName);

            if (user is null) { return false; }

            return !string.IsNullOrWhiteSpace(user.TimeZone);
        }

        public bool IsUserRegisteredToSignUpToWoWEvents(string discordUserName)
        {
            EchelonUser? user = GetUser(discordUserName);

            if (user is null) { return false; }

            return !string.IsNullOrWhiteSpace(user.Class) && !string.IsNullOrWhiteSpace(user.Spec);
        }

        // Scheduled Messages

        public async Task UpsertScheduledMessageAsync(ScheduledMessage message)
        {
            ScheduledMessageEntity entity = new()
            {
                PartitionKey = "ScheduledMessages",
                RowKey = message.EventId.ToString(),

                EventId = message.EventId,
                Message = message.Message,
                SendTime = message.SendTime,
                UserId = message.UserId
            };

            await _scheduledMessageTable.UpsertEntityAsync(entity);
        }

        public IEnumerable<ScheduledMessage>? GetScheduledMessages(ulong messageId)
        {
            IEnumerable<ScheduledMessageEntity>? entities = _scheduledMessageTable.Query<ScheduledMessageEntity>(e => e.RowKey == messageId.ToString());

            foreach (ScheduledMessageEntity entity in entities)
            {
                var message = new ScheduledMessage()
                {
                    EventId = entity.EventId,
                    Message = entity.Message,
                    SendTime = entity.SendTime,
                    UserId = entity.UserId
                };

                yield return message;
            }
        }

        public async Task DeleteScheduledMessageAsync(ScheduledMessage message)
        {
            ScheduledMessageEntity? entity = _scheduledMessageTable.Query<ScheduledMessageEntity>(e => e.RowKey == message.EventId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _scheduledMessageTable.DeleteEntityAsync(entity);
        }

        // Emotes

        public async Task UpsertEmoteAsync(StoredEmote emote)
        {
            StoredEmoteEntity entity = new()
            {
                PartitionKey = emote.ClassName,
                RowKey = emote.SpecName,

                ClassName = emote.ClassName,
                SpecName = emote.SpecName,
                EmoteID = emote.EmoteID
            };

            await _storedEmoteTable.UpsertEntityAsync(entity);
        }

        public StoredEmote? GetEmote(string className, string spec)
        {
            StoredEmoteEntity? entity = _storedEmoteTable.Query<StoredEmoteEntity>(e => e.ClassName == className && e.SpecName == spec).FirstOrDefault();

            if (entity is null) { return null; }

            StoredEmote emote = new()
            {
                EmoteID = entity.EmoteID,
                ClassName = entity.ClassName,
                SpecName = entity.SpecName
            };

            return emote;
        }

        public async Task DeleteEmoteAsync(StoredEmote emote)
        {
            StoredEmoteEntity? entity = _storedEmoteTable.Query<StoredEmoteEntity>(e => e.ClassName == emote.ClassName && e.SpecName == emote.SpecName).FirstOrDefault();

            if (entity is null) { return; }

            await _storedEmoteTable.DeleteEntityAsync(entity);
        }

        // WoW Characters

        public async Task UpsertWowCharacter(WoWCharacter wowCharacter)
        {
            WoWCharacterEntity entity = new()
            {
                PartitionKey = wowCharacter.Class,
                RowKey = wowCharacter.Id.ToString(),

                CharacterName = wowCharacter.CharacterName,
                CharacterRealm = wowCharacter.CharacterRealm,
                Class = wowCharacter.Class,
                Id = wowCharacter.Id,
                OffSpec = wowCharacter.OffSpec,
                RegisteredTo = wowCharacter.RegisteredTo,
                Specialization = wowCharacter.Specialization
            };

            await _wowCharacterTable.UpsertEntityAsync(entity);
        }

        public WoWCharacter? GetWoWCharacter(string characterName, string characterRealm)
        {
            WoWCharacterEntity? entity = _wowCharacterTable.Query<WoWCharacterEntity>(e => e.CharacterName == characterName && e.CharacterRealm == characterRealm).FirstOrDefault();

            if (entity is null) { return null; }

            WoWCharacter character = new()
            {
                Id = entity.Id,
                CharacterName = characterName,
                CharacterRealm = characterRealm,
                Class = entity.Class,
                OffSpec = entity.OffSpec,
                RegisteredTo = entity.RegisteredTo,
                Specialization = entity.Specialization                
            };

            return character;
        }

        public async Task DeleteCharacterAsync(string characterName, string characterRealm)
        {
            WoWCharacterEntity? entity = _wowCharacterTable.Query<WoWCharacterEntity>(e => e.CharacterName == characterName && e.CharacterRealm == characterRealm).FirstOrDefault();

            if (entity is null) { return; }

            await _wowCharacterTable.DeleteEntityAsync(entity);
        }

        // WoW Instance

        public async Task UpsertInstanceInfo(WoWInstanceInfo instanceInfo)
        {
            WoWInstanceInfoEntity entity = new()
            {
                PartitionKey = instanceInfo.InstanceType.ToString(),
                RowKey = instanceInfo.Name,

                ImageUrl = instanceInfo.ImageUrl,
                InstanceType = instanceInfo.InstanceType,
                Legacy = instanceInfo.Legacy,
                Name = instanceInfo.Name
            };

            await _wowInstanceInfoTable.UpsertEntityAsync(entity);
        }

        public WoWInstanceInfo? GetInstanceInfo(string instanceName)
        {
            WoWInstanceInfoEntity? entity = _wowInstanceInfoTable.Query<WoWInstanceInfoEntity>(e => e.Name == instanceName).FirstOrDefault();

            if (entity is null) { return null; }

            WoWInstanceInfo instance = new()
            {
                ImageUrl = entity.ImageUrl,
                InstanceType = entity.InstanceType,
                Legacy = entity.Legacy,
                Name = entity.Name                
            };

            return instance;
        }

        public async Task DeleteInstanceInfoAsync(string instanceName)
        {
            WoWInstanceInfoEntity? entity = _wowInstanceInfoTable.Query<WoWInstanceInfoEntity>(e => e.Name == instanceName).FirstOrDefault();

            if (entity is null) { return; }

            await _wowInstanceInfoTable.DeleteEntityAsync(entity);
        }


        // WoW Team
        public async Task UpsertTeamAsync(WoWTeam team)
        {
            WoWTeamEntity entity = new()
            {
                PartitionKey = "",
                RowKey = "",

                Description = team.Description,
                DisplayName = team.DisplayName,
                ForInstanceType = team.ForInstanceType,
                Name = team.Name
            };

            await _wowTeamTable.UpsertEntityAsync(entity);
        }

        public WoWTeam? GetTeam(string teamName)
        {
            WoWTeamEntity? entity = _wowTeamTable.Query<WoWTeamEntity>(e => e.Name == teamName).FirstOrDefault();

            if (entity is null) { return null; }

            WoWTeam team = new()
            {
                ForInstanceType = entity.ForInstanceType,
                Name = entity.Name,
                Description = entity.Description,
                DisplayName = entity.DisplayName
            };

            return team;
        }

        public async Task DeleteTeamAsync(string teamName)
        {
            WoWTeamEntity? entity = _wowTeamTable.Query<WoWTeamEntity>(e => e.Name == teamName).FirstOrDefault();

            if (entity is null) { return; }

            await _wowTeamTable.DeleteEntityAsync(entity);
        }
        
    }
}
