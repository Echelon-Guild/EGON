using Azure.Core;
using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Models.Modals;
using EGON.DiscordBot.Services;
using NodaTime;

namespace EGON.DiscordBot.Modules
{
    // This had to be split in two because of the [Group()] tag.

    [Group("edit", "Edit an event")]
    public class EditModuleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;
        private readonly BlobUploadService _blobUploadService;
        private readonly EmbedFactory _embedFactory;

        public EditModuleCommands(StorageService storageService, BlobUploadService blobUploadService, EmbedFactory embedFactory)
        {
            _storageService = storageService;
            _blobUploadService = blobUploadService;
            _embedFactory = embedFactory;
        }

        [SlashCommand("date", "Edit the date/time of an event")]
        public async Task EditDate(string eventId)
        {
            ulong id;

            if (!ulong.TryParse(eventId, out id))
            {
                await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondWithModalAsync<ChangeEventDateModal>($"change_event_date_{eventId}");
        }

        [SlashCommand("name", "Edit the name of an event")]
        public async Task EditName(string eventId)
        {
            ulong id;

            if (!ulong.TryParse(eventId, out id))
            {
                await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondWithModalAsync<ChangeEventNameModal>($"change_event_name_{eventId}");
        }

        [SlashCommand("description", "Edit the description of an event")]
        public async Task EditDescription(string eventId)
        {
            ulong id;

            if (!ulong.TryParse(eventId, out id))
            {
                await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondWithModalAsync<ChangeEventDescriptionModal>($"change_event_description_{eventId}");
        }

        [SlashCommand("image", "Edit the image of an event")]
        public async Task EditImage(string eventId, IAttachment image)
        {
            ulong id;

            if (!ulong.TryParse(eventId, out id))
            {
                await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondAsync("Updating now!", ephemeral: true);

            Uri blobUri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

            event_.ImageUrl = blobUri.ToString();

            await UpdateEventEmbed(id);
        }

        private async Task UpdateEventEmbed(ulong eventId, bool cancelled = false)
        {
            // Retrieve event entity (including MessageId)
            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                return;
            }

            // Retrieve the Discord message
            var channel = Context.Client.GetChannel(event_.ChannelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(event_.MessageId) as IUserMessage;

            if (message is null)
            {
                return;
            }

            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

            Embed? embed;

            if (cancelled)
                embed = _embedFactory.CreateCancelledEventEmbed(event_);
            else
                embed = _embedFactory.CreateEventEmbed(event_, attendees);

            // Modify the existing message with the updated embed
            await message.ModifyAsync(msg =>
            {
                msg.Embed = embed;

                if (cancelled)
                    msg.Components = new ComponentBuilder().Build();
            });
        }
    }

    public class EditModuleResponses : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;
        private readonly EmbedFactory _embedFactory;

        public EditModuleResponses(StorageService storageService, EmbedFactory embedFactory)
        {
            _storageService = storageService;
            _embedFactory = embedFactory;
        }

        [ModalInteraction("change_event_date_*")]
        public async Task HandleChangeEventDate(string customId, ChangeEventDateModal modal)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            string? organizerTimeZone = _storageService.GetUser(Context.User.Username)?.TimeZone;

            if (organizerTimeZone is null)
            {
                await RespondAsync("Tell Chris something weird happened in HandleChangeEventDate.", ephemeral: true);
                return;
            }

            DateTimeZone tz = DateTimeZoneProviders.Tzdb[organizerTimeZone];

            // 2. Create a LocalDateTime (no offset yet)
            var localDateTime = new LocalDateTime(modal.DateTimeOfEvent.Year,
                                                  modal.DateTimeOfEvent.Month,
                                                  modal.DateTimeOfEvent.Day,
                                                  modal.DateTimeOfEvent.Hour,
                                                  modal.DateTimeOfEvent.Minute, 0);

            var zonedDateTime = tz.AtStrictly(localDateTime);

            var offsetDateTime = zonedDateTime.ToOffsetDateTime();

            DateTimeOffset eventDateTime = offsetDateTime.ToDateTimeOffset();

            if (eventDateTime < DateTime.UtcNow)
            {
                await RespondAsync("Can't schedule an event in the past!", ephemeral: true);
                return;
            }

            await RespondAsync("Updating now!", ephemeral: true);

            event_.EventDateTime = eventDateTime;

            await _storageService.UpsertEventAsync(event_);

            await UpdateEventEmbed(event_.Id);
        }

        [ModalInteraction("change_event_name_*")]
        public async Task HandleChangeEventName(string customId, ChangeEventNameModal modal)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondAsync("Updating now!", ephemeral: true);

            event_.Name = modal.Name;

            await _storageService.UpsertEventAsync(event_);

            await UpdateEventEmbed(event_.Id);
        }

        [ModalInteraction("change_event_description_*")]
        public async Task HandleChangeEventDescription(string customId, ChangeEventDescriptionModal modal)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("Can't find that event. It was probably deleted.", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync("This isn't your event. Only the organizer can edit an event.", ephemeral: true);
                return;
            }

            await RespondAsync("Updating now!", ephemeral: true);

            event_.Description = modal.Description;

            await _storageService.UpsertEventAsync(event_);

            await UpdateEventEmbed(event_.Id);
        }

        // This should be on it's own
        private async Task UpdateEventEmbed(ulong eventId, bool cancelled = false)
        {
            // Retrieve event entity (including MessageId)
            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                return;
            }

            // Retrieve the Discord message
            var channel = Context.Client.GetChannel(event_.ChannelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(event_.MessageId) as IUserMessage;

            if (message is null)
            {
                return;
            }

            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

            Embed? embed;

            if (cancelled)
                embed = _embedFactory.CreateCancelledEventEmbed(event_);
            else
                embed = _embedFactory.CreateEventEmbed(event_, attendees);

            // Modify the existing message with the updated embed
            await message.ModifyAsync(msg =>
            {
                msg.Embed = embed;

                if (cancelled)
                    msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}
