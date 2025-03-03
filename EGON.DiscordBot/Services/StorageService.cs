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
        private readonly TableClient _approvedCallerTable;
        private readonly TableClient _scheduledPostTable;
        private readonly TableClient _egonSettingsTable;

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

            _approvedCallerTable = tableServiceClient.GetTableClient(TableNames.APPROVED_CALLER_TABLE_NAME);
            _approvedCallerTable.CreateIfNotExists();

            _scheduledPostTable = tableServiceClient.GetTableClient(TableNames.SCHEDULED_POST_TABLE_NAME);
            _scheduledPostTable.CreateIfNotExists();

            _egonSettingsTable = tableServiceClient.GetTableClient(TableNames.EGON_SETTINGS_TABLE_NAME);
            _egonSettingsTable.CreateIfNotExists();
        }

        // EGON Settings
        public async Task UpsertEGONSettingAsync(EGONSetting setting)
        {
            var entity = new EGONSettingEntity(setting);

            await _egonSettingsTable.UpsertEntityAsync(entity);
        }

        public EGONSetting? GetSetting(string name)
        {
            EGONSettingEntity? entity = _egonSettingsTable.Query<EGONSettingEntity>(e => e.Name == name).FirstOrDefault();

            return entity?.ToDTO();
        }

        public async Task DeleteSettingAsync(EGONSetting setting)
        {
            EGONSettingEntity? entity = _egonSettingsTable.Query<EGONSettingEntity>(e => e.Name == setting.Name).FirstOrDefault();

            if (entity is null) { return; }

            await _egonSettingsTable.DeleteEntityAsync(entity);
        }


        // Scheduled post
        public async Task UpsertScheduledPostAsync(ScheduledPost scheduledPost)
        {
            var entity = new ScheduledPostEntity(scheduledPost);

            await _scheduledPostTable.UpsertEntityAsync(entity);
        }

        public IEnumerable<ScheduledPost>? GetPostsToSend()
        { 
            IEnumerable<ScheduledPostEntity> entities = _scheduledPostTable.Query<ScheduledPostEntity>(e => e.SendTime <= DateTime.UtcNow);

            foreach (ScheduledPostEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        public async Task DeletePostAsync(ScheduledPost post)
        {
            // This avoids wonkiness in the table query.
            string eventId = post.EventId.ToString();

            ScheduledPostEntity? entity = _scheduledPostTable.Query<ScheduledPostEntity>(e => e.RowKey == eventId).FirstOrDefault();

            if (entity is null) { return; }

            await _scheduledPostTable.DeleteEntityAsync(entity);
        }

        // Approved caller

        public bool IsApprovedCaller(string discordUserName, string commandName)
        {
            if (string.IsNullOrWhiteSpace(discordUserName))
                return false;

            if (string.IsNullOrWhiteSpace(commandName))
                return false;

            if (discordUserName == "chris068367")
                return true;

            string commandNameToLower = commandName.ToLower();

            IEnumerable<ApprovedCallerEntity>? approvedCallers = _approvedCallerTable.Query<ApprovedCallerEntity>(e => e.DiscordUserName == discordUserName && (e.AuthorizedToCallCommandName == commandName || e.AuthorizedToCallCommandName == "all"));

            return approvedCallers?.Any() ?? false;
        }

        // Events

        public async Task UpsertEventAsync(EchelonEvent ecEvent)
        {
            var entity = new EchelonEventEntity(ecEvent);

            await _eventTable.UpsertEntityAsync(entity);
        }

        public EchelonEvent? GetEvent(ulong eventId)
        {
            EchelonEventEntity? entity = _eventTable.Query<EchelonEventEntity>(e => e.EventId == eventId.ToString()).FirstOrDefault();

            if (entity is null) { return null; }

            return entity.ToDTO();

        }

        public async Task DeleteEventAsync(EchelonEvent ecEvent)
        {
            EchelonEventEntity? entity = _eventTable.Query<EchelonEventEntity>(e => e.RowKey == ecEvent.MessageId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _eventTable.DeleteEntityAsync(entity);
        }

        public IEnumerable<EchelonEvent>? GetUpcomingEvent()
        {
            IEnumerable<EchelonEventEntity>? entities = _eventTable.Query<EchelonEventEntity>(e => e.EventDateTime > DateTimeOffset.UtcNow);

            foreach (EchelonEventEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        public IEnumerable<EchelonEvent>? GetEventsToClose()
        {
            IEnumerable<EchelonEventEntity>? entities = _eventTable.Query<EchelonEventEntity>(e => e.EventDateTime <= DateTimeOffset.UtcNow && !e.Closed);

            foreach (EchelonEventEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        public async Task CancelEventAsync(ulong eventId)
        {
            EchelonEvent? ecEvent = GetEvent(eventId);

            IEnumerable<AttendeeRecord>? attendees = GetAttendeeRecords(eventId);

            IEnumerable<ScheduledMessage>? scheduledMessages = GetScheduledMessages(eventId);

            if (ecEvent is not null)
                await DeleteEventAsync(ecEvent);

            List<Task> tasks = new();

            if (attendees is not null && attendees.Any())
            {
                foreach (AttendeeRecord attendee in attendees)
                {
                    tasks.Add(DeleteAttendeeRecordAsync(attendee));
                }
            }

            if (scheduledMessages is not null && scheduledMessages.Any())
            {
                foreach (ScheduledMessage message in scheduledMessages)
                {
                    tasks.Add(DeleteScheduledMessageAsync(message));
                }
            }

            await Task.WhenAll(tasks);
        }

        // Attendees

        public async Task UpsertAttendeesAsync(IEnumerable<AttendeeRecord> attendeeRecords)
        {
            List<Task> tasks = new();

            foreach (AttendeeRecord record in attendeeRecords)
            {
                var entity = new AttendeeRecordEntity(record);

                tasks.Add(_attendeeRecordTable.UpsertEntityAsync(entity));
            }

            await Task.WhenAll(tasks);
        }

        public async Task UpsertAttendeeAsync(AttendeeRecord record)
        {
            var entity = new AttendeeRecordEntity(record);

            await _attendeeRecordTable.UpsertEntityAsync(entity);
        }

        public IEnumerable<AttendeeRecord>? GetAttendeeRecords(ulong messageId)
        {
            IEnumerable<AttendeeRecordEntity> records = _attendeeRecordTable.Query<AttendeeRecordEntity>(e => e.PartitionKey == messageId.ToString());

            foreach (AttendeeRecordEntity record in records)
            {
                yield return record.ToDTO();
            }
        }

        public async Task DeleteAttendeeRecordAsync(AttendeeRecord record)
        {
            AttendeeRecordEntity? entity = _attendeeRecordTable.Query<AttendeeRecordEntity>(e => e.DiscordName == record.DiscordName && e.PartitionKey == record.EventId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _attendeeRecordTable.DeleteEntityAsync(entity);
        }

        // Users

        public async Task UpsertUserAsync(EchelonUser user)
        {
            var entity = new EchelonUserEntity(user);

            await _echelonUserTable.UpsertEntityAsync(entity);
        }

        public EchelonUser? GetUser(string discordUserName)
        {
            EchelonUserEntity? entity = _echelonUserTable.Query<EchelonUserEntity>(e => e.RowKey == discordUserName).FirstOrDefault();

            if (entity is null) { return null; }

            return entity.ToDTO();
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
            var entity = new ScheduledMessageEntity(message);

            await _scheduledMessageTable.UpsertEntityAsync(entity);
        }

        public IEnumerable<ScheduledMessage>? GetScheduledMessages(ulong eventId, ulong userId)
        {
            IEnumerable<ScheduledMessageEntity>? entities = _scheduledMessageTable.Query<ScheduledMessageEntity>(e => e.EventId == eventId.ToString() && e.UserId == userId.ToString());

            foreach (ScheduledMessageEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        public IEnumerable<ScheduledMessage>? GetScheduledMessages(ulong eventId)
        {
            IEnumerable<ScheduledMessageEntity>? entities = _scheduledMessageTable.Query<ScheduledMessageEntity>(e => e.EventId == eventId.ToString());

            foreach (ScheduledMessageEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        public async Task DeleteScheduledMessageAsync(ScheduledMessage message)
        {
            ScheduledMessageEntity? entity = _scheduledMessageTable.Query<ScheduledMessageEntity>(e => e.RowKey == message.EventId.ToString()).FirstOrDefault();

            if (entity is null) { return; }

            await _scheduledMessageTable.DeleteEntityAsync(entity);
        }

        public IEnumerable<ScheduledMessage> GetMessagesToSend()
        {
            var now = DateTimeOffset.UtcNow;

            var entities = _scheduledMessageTable.Query<ScheduledMessageEntity>(m => m.SendTime <= now);

            foreach (ScheduledMessageEntity entity in entities)
            {
                yield return entity.ToDTO();
            }
        }

        // Emotes

        public async Task UpsertEmoteAsync(StoredEmote emote)
        {
            var entity = new StoredEmoteEntity(emote);

            await _storedEmoteTable.UpsertEntityAsync(entity);
        }

        public StoredEmote? GetEmote(string className, string spec)
        {
            StoredEmoteEntity? entity = _storedEmoteTable.Query<StoredEmoteEntity>(e => e.ClassName == className && e.SpecName == spec).FirstOrDefault();

            if (entity is null) { return null; }

            return entity.ToDTO();
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
            var entity = new WoWCharacterEntity(wowCharacter);

            await _wowCharacterTable.UpsertEntityAsync(entity);
        }

        public WoWCharacter? GetWoWCharacter(string characterName, string characterRealm)
        {
            WoWCharacterEntity? entity = _wowCharacterTable.Query<WoWCharacterEntity>(e => e.CharacterName == characterName && e.CharacterRealm == characterRealm).FirstOrDefault();

            if (entity is null) { return null; }

            return entity.ToDTO();
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
            var entity = new WoWInstanceInfoEntity(instanceInfo);

            await _wowInstanceInfoTable.UpsertEntityAsync(entity);
        }

        public WoWInstanceInfo? GetInstanceInfo(string instanceName)
        {
            WoWInstanceInfoEntity? entity = _wowInstanceInfoTable.Query<WoWInstanceInfoEntity>(e => e.Name == instanceName).FirstOrDefault();

            if (entity is null) { return null; }

            return entity.ToDTO();
        }

        public async Task DeleteInstanceInfoAsync(string instanceName)
        {
            WoWInstanceInfoEntity? entity = _wowInstanceInfoTable.Query<WoWInstanceInfoEntity>(e => e.Name == instanceName).FirstOrDefault();

            if (entity is null) { return; }

            await _wowInstanceInfoTable.DeleteEntityAsync(entity);
        }
    }
}
