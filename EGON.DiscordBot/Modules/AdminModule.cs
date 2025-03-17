using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Models.Modals;
using EGON.DiscordBot.Services;
using System.Text;

namespace EGON.DiscordBot.Modules
{
    [Group("admin", "Admin functionality for E.G.O.N.")]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("attend", "Modify attendance records for an event.")]
        public class AdminAttendModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly StorageService _storageService;
            private readonly EmbedUpdateService _embedUpdateService;

            public AdminAttendModule(StorageService storageService, EmbedUpdateService embedUpdateService)
            {
                _storageService = storageService;
                _embedUpdateService = embedUpdateService;
            }

            [SlashCommand("add", "Add an attendance record for a user to an event.")]
            public async Task AdminAttendAdd(string userId, string eventId, string? userName = "", string? userClass = "", string? userSpec = "")
            {
                ulong _eventId = ulong.Parse(eventId);

                EchelonEvent? event_ = _storageService.GetEvent(_eventId);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not registered.", ephemeral: true);
                    return;
                }

                EchelonUser? user = _storageService.GetUser(userId);

                if (event_.EventType != EventType.Event)
                {
                    if (user is null)
                    {
                        if (string.IsNullOrWhiteSpace(userClass) ||
                            string.IsNullOrWhiteSpace(userSpec) ||
                            string.IsNullOrWhiteSpace(userName))
                        {
                            await RespondAsync($"User {userId} is not in the database, so you must supply display name, class, and spec.", ephemeral: true);
                            return;
                        }

                        user = new EchelonUser()
                        {
                            Class = userClass.Replace(' ', '_'),
                            Spec = userSpec.Replace(' ', '_'),
                            DiscordDisplayName = userName,
                            DiscordName = userId
                        };

                        await _storageService.UpsertUserAsync(user);
                    }
                }
                else
                {
                    if (user is null)
                    {
                        if (string.IsNullOrWhiteSpace(userName))
                        {
                            await RespondAsync($"User {userId} is not in the database, so you must supply user's display name.", ephemeral: true);
                            return;
                        }

                        user = new EchelonUser()
                        {
                            DiscordDisplayName = userName,
                            DiscordName = userId
                        };

                        await _storageService.UpsertUserAsync(user);
                    }
                }

                await RespondAsync("Uploading record now.", ephemeral: true);

                AttendeeRecord? attendeeRecord = _storageService.GetAttendeeRecords(event_.MessageId)?.Where(e => e.DiscordName == userId).FirstOrDefault();

                if (attendeeRecord is null)
                    attendeeRecord = new();

                attendeeRecord.EventId = event_.Id;
                attendeeRecord.DiscordName = userId;
                attendeeRecord.DiscordDisplayName = userName;

                if (event_.EventType == EventType.Event)
                    attendeeRecord.Role = "Attendee";
                else
                {
                    attendeeRecord.Role = GetRole(user.Class, user.Spec);
                }

                await _storageService.UpsertAttendeeAsync(attendeeRecord);

                await _embedUpdateService.UpdateEventEmbed(_eventId);
            }

            [SlashCommand("remove", "Remove an attendance record for a user to an event.")]
            public async Task AdminAttendRemove(string userId, string eventId)
            {
                ulong _eventId = ulong.Parse(eventId);

                EchelonEvent? event_ = _storageService.GetEvent(_eventId);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not registered.", ephemeral: true);
                    return;
                }

                IEnumerable<AttendeeRecord>? records = _storageService.GetAttendeeRecords(_eventId)?.Where(e => e.DiscordName == userId);

                if (records is null || !records.Any())
                {
                    await RespondAsync($"No records found for {userId} in event {eventId}", ephemeral: true);
                    return;
                }

                await RespondAsync("Removing now.", ephemeral: true);

                foreach (AttendeeRecord record in records)
                {
                    await _storageService.DeleteAttendeeRecordAsync(record);
                }

                await _embedUpdateService.UpdateEventEmbed(event_.Id);
            }

            private string GetRole(string playerClass, string spec)
            {
                var tanks = new HashSet<string> { "Blood Death Knight", "Guardian Druid", "Brewmaster Monk", "Protection Paladin", "Protection Warrior", "Vengeance Demon Hunter" };
                var healers = new HashSet<string> { "Restoration Druid", "Mistweaver Monk", "Holy Paladin", "Holy Priest", "Discipline Priest", "Restoration Shaman", "Preservation Evoker" };
                var mDps = new HashSet<string>
            {
                "Assassination Rogue",
                "Outlaw Rogue",
                "Subtlety Rogue",
                "Fury Warrior",
                "Arms Warrior",
                "Retribution Paladin",
                "Frost Death Knight",
                "Unholy Death Knight",
                "Enhancement Shaman",
                "Feral Druid",
                "Havoc Demon Hunter",
                "Windwalker Monk",
                "Survival Hunter"
            };

                string fullSpec = $"{spec.Prettyfy()} {playerClass.Prettyfy()}";

                if (tanks.Contains(fullSpec)) return "Tank";
                if (healers.Contains(fullSpec)) return "Healer";
                if (mDps.Contains(fullSpec)) return "Melee DPS";
                return "Ranged DPS";
            }
        }

        [Group("image", "Update the default image used when creating an event.")]
        public class ImageSettingCommands : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly StorageService _storageService;
            private readonly BlobUploadService _blobUploadService;

            public ImageSettingCommands(StorageService storageService, BlobUploadService blobUploadService)
            {
                _storageService = storageService;
                _blobUploadService = blobUploadService;
            }

            [SlashCommand("mythic", "Update the default image used when creating a Mythic event.")]
            public async Task Mythic(IAttachment image)
            {
                string userName = Context.User.Username;

                if (userName != "chris068367" && userName != "fadedskyjeff")
                {
                    await RespondAsync("You are not authorized to update default images", ephemeral: true);
                    return;
                }

                await RespondAsync("Uploading now!", ephemeral: true);

                Uri uri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                EGONSetting setting = new()
                {
                    Name = "default-mythic",
                    Value = uri.ToString()
                };

                await _storageService.UpsertEGONSettingAsync(setting);
            }

            [SlashCommand("raid", "Update the default image used when creating a Raid event.")]
            public async Task Raid(IAttachment image)
            {
                string userName = Context.User.Username;

                if (userName != "chris068367" && userName != "fadedskyjeff" && userName != "sinister01")
                {
                    await RespondAsync("You are not authorized to update default images", ephemeral: true);
                    return;
                }

                await RespondAsync("Uploading now!", ephemeral: true);

                Uri uri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                EGONSetting setting = new()
                {
                    Name = "default-raid",
                    Value = uri.ToString()
                };

                await _storageService.UpsertEGONSettingAsync(setting);
            }

            [SlashCommand("event", "Update the default image used when creating a non-WoW event.")]
            public async Task Event(IAttachment image)
            {
                string userName = Context.User.Username;

                if (userName != "chris068367" && userName != "fadedskyjeff")
                {
                    await RespondAsync("You are not authorized to update default images", ephemeral: true);
                    return;
                }

                await RespondAsync("Uploading now!", ephemeral: true);

                Uri uri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                EGONSetting setting = new()
                {
                    Name = "default-event",
                    Value = uri.ToString()
                };

                await _storageService.UpsertEGONSettingAsync(setting);
            }

        }

        [Group("footer", "Add, update, or delete an event footer.")]
        public class FooterSettingCommands : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly StorageService _storageService;

            public FooterSettingCommands(StorageService storageService)
            {
                _storageService = storageService;
            }

            [SlashCommand("add", "Add a possible event footer.")]
            public async Task AddFooter(string footer)
            {
                if (!_storageService.IsApprovedCaller(Context.User.Username, "footer"))
                {
                    await RespondAsync("You aren't authorized to add or remove footers! Sorry!", ephemeral: true);
                    return;
                }

                Footer? matchingFooter = _storageService.GetFooter(footer);

                if (matchingFooter is not null)
                {
                    await RespondAsync("Footer with id {matchingFooter.Id} already has this text.", ephemeral: true);
                    return;
                }

                Footer item = new()
                {
                    Id = Guid.NewGuid(),
                    Value = footer
                };

                await _storageService.UpsertFooterAsync(item);

                await RespondAsync("Added!", ephemeral: true);
            }

            [SlashCommand("remove", "Remove a possible event footer.")]
            public async Task RemoveFooter(string id)
            {
                if (!Guid.TryParse(id, out Guid id_))
                {
                    await RespondAsync($"{id} is not a valid footer id.", ephemeral: true);
                    return;
                }

                if (!_storageService.IsApprovedCaller(Context.User.Username, "footer"))
                {
                    await RespondAsync("You aren't authorized to add or remove footers! Sorry!", ephemeral: true);
                    return;
                }

                Footer? footer = _storageService.GetFooter(id_);

                if (footer is null)
                {
                    await RespondAsync("No footer found with that id", ephemeral: true);
                    return;
                }

                await _storageService.DeleteFooterAsync(footer);

                await RespondAsync("Removed!", ephemeral: true);
            }

            [SlashCommand("list", "List the stored footers.")]
            public async Task ListFooter()
            {
                StringBuilder sb = new();

                foreach (Footer item in _storageService.GetFooters() ?? [])
                {
                    sb.AppendLine($"{item.Id} - {item.Value}");
                }

                await RespondAsync(sb.ToString(), ephemeral: true);
            }

            [SlashCommand("update", "Update a stored footer with new text.")]
            public async Task UpdateFooter(string id, string footer)
            {
                if (!Guid.TryParse(id, out Guid id_))
                {
                    await RespondAsync($"{id} is not a valid footer id.", ephemeral: true);
                    return;
                }

                if (!_storageService.IsApprovedCaller(Context.User.Username, "footer"))
                {
                    await RespondAsync("You aren't authorized to add or remove footers! Sorry!", ephemeral: true);
                    return;
                }

                Footer? item = _storageService.GetFooter(id_);

                if (item is null)
                {
                    await RespondAsync("No footer found with that id", ephemeral: true);
                    return;
                }

                item.Value = footer;

                await _storageService.UpsertFooterAsync(item);

                await RespondAsync("Updated!", ephemeral: true);
            }
        }

        [Group("event", "Update or delete an event.")]
        public class AdminEventModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly StorageService _storageService;
            private readonly BlobUploadService _blobUploadService;
            private readonly EmbedUpdateService _embedUpdateService;

            public AdminEventModule(StorageService storageService, BlobUploadService blobUploadService, EmbedUpdateService embedUpdateService)
            {
                _storageService = storageService;
                _blobUploadService = blobUploadService;
                _embedUpdateService = embedUpdateService;
            }

            [SlashCommand("edit", "Edit an event.")]
            public async Task AdminEventEdit(string eventId)
            {
                if (!ulong.TryParse(eventId, out ulong id))
                {
                    await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                    return;
                }

                EchelonEvent? event_ = _storageService.GetEvent(id);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not found.", ephemeral: true);
                    return;
                }

                var modalBuilder = new ModalBuilder()
                    .WithCustomId($"admin_edit_event_{eventId}")
                    .WithTitle($"Edit event {eventId}")
                    .AddTextInput("Name", "Name", placeholder: event_.Name, required: false)
                    .AddTextInput("Description", "Description", TextInputStyle.Paragraph, placeholder: event_.Description, required: false)
                    .AddTextInput("Date/Time of Event", "dateTimeOfEvent", placeholder: event_.EventDateTime.ToString("MM/dd/yyyy hh:mm tt"), required: false);

                await RespondWithModalAsync(modalBuilder.Build());
            }

            [SlashCommand("delete", "Delete an event.")]
            public async Task AdminEventDelete(string eventId)
            {
                if (!ulong.TryParse(eventId, out ulong id))
                {
                    await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                    return;
                }

                EchelonEvent? event_ = _storageService.GetEvent(id);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not found.", ephemeral: true);
                    return;
                }

                var areYouSure = new SelectMenuBuilder()
                    .WithCustomId($"admin_delete_event_{eventId}")
                    .WithPlaceholder("Are you sure?")
                    .AddOption("Yes", "yes")
                    .AddOption("No", "no");

                var builder = new ComponentBuilder().WithSelectMenu(areYouSure);

                await RespondAsync("Are you sure?", components: builder.Build(), ephemeral: true);
            }

            [SlashCommand("cancel", "Cancel an event.")]
            public async Task AdminEventCancel(string eventId)
            {
                if (!ulong.TryParse(eventId, out ulong id))
                {
                    await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                    return;
                }

                EchelonEvent? event_ = _storageService.GetEvent(id);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not found.", ephemeral: true);
                    return;
                }

                var areYouSure = new SelectMenuBuilder()
                    .WithCustomId($"admin_cancel_event_{eventId}")
                    .WithPlaceholder("Are you sure?")
                    .AddOption("Yes", "yes")
                    .AddOption("No", "no");

                var builder = new ComponentBuilder().WithSelectMenu(areYouSure);

                await RespondAsync("Are you sure?", components: builder.Build(), ephemeral: true);
            }

            [SlashCommand("setimage", "Set the image of an event.")]
            public async Task AdminEventSetImage(string eventId, IAttachment image)
            {
                if (!ulong.TryParse(eventId, out ulong id))
                {
                    await RespondAsync($"{eventId} is not a valid event id.", ephemeral: true);
                    return;
                }

                EchelonEvent? event_ = _storageService.GetEvent(id);

                if (event_ is null)
                {
                    await RespondAsync($"Event {eventId} not found.", ephemeral: true);
                    return;
                }

                await RespondAsync($"Updating image for event {eventId}", ephemeral: true);

                Uri blobUri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                event_.ImageUrl = blobUri.ToString();

                await _storageService.UpsertEventAsync(event_);

                await _embedUpdateService.UpdateEventEmbed(event_.Id);
            }
        }
    }

    public class AdminModuleInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;
        private readonly EmbedUpdateService _embedUpdateService;

        public AdminModuleInteractions(StorageService storageService, EmbedUpdateService embedUpdateService)
        {
            _storageService = storageService;
            _embedUpdateService = embedUpdateService;
        }

        [ModalInteraction("admin_edit_event_*")]
        public async Task HandleAdminEditEvent(string customId, EditEventModal modal)
        {
            if (!ulong.TryParse(customId, out ulong id))
            {
                await RespondAsync($"{customId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync($"Event {customId} not found.", ephemeral: true);
                return;
            }

            await RespondAsync("Updating!", ephemeral: true);

            if (!string.IsNullOrWhiteSpace(modal.Name))
                event_.Name = modal.Name;

            if (!string.IsNullOrWhiteSpace(modal.Description))
                event_.Description = modal.Description;

            if (!string.IsNullOrWhiteSpace(modal.DateTimeOfEvent))
            {
                if (!DateTime.TryParse(modal.DateTimeOfEvent, out DateTime dt))
                {
                    await RespondAsync($"{modal.DateTimeOfEvent} is not a valid datetime.", ephemeral: true);
                    return;
                }
            }

            await _storageService.UpsertEventAsync(event_);

            await _embedUpdateService.UpdateEventEmbed(event_.Id);
        }

        [ComponentInteraction("admin_delete_event_*")]
        public async Task HandleAdminDeleteEvent(string customId, string yesOrNo)
        {
            if (yesOrNo != "yes")
            {
                await RespondAsync("Phew. That was close.", ephemeral: true);
                return;
            }

            if (!ulong.TryParse(customId, out ulong id))
            {
                await RespondAsync($"{customId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync($"Event {customId} not found.", ephemeral: true);
                return;
            }

            await RespondAsync("Deleting.", ephemeral: true);

            var channel = Context.Client.GetChannel(event_.ChannelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(event_.MessageId) as IUserMessage;

            await message.DeleteAsync();

            IEnumerable<AttendeeRecord>? records = _storageService.GetAttendeeRecords(event_.MessageId);
            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(event_.MessageId);

            List<Task> tasks = new();

            foreach (AttendeeRecord record in records)
            {
                tasks.Add(_storageService.DeleteAttendeeRecordAsync(record));
            }

            foreach (ScheduledMessage msg in messages)
            {
                tasks.Add(_storageService.DeleteScheduledMessageAsync(msg));
            }

            tasks.Add(_storageService.DeleteEventAsync(event_));

            await Task.WhenAll(tasks);
        }

        [ComponentInteraction("admin_cancel_event_*")]
        public async Task HandleAdminCancelEvent(string customId, string yesOrNo)
        {
            if (yesOrNo != "yes")
            {
                await RespondAsync("Phew. That was close.", ephemeral: true);
                return;
            }

            if (!ulong.TryParse(customId, out ulong id))
            {
                await RespondAsync($"{customId} is not a valid event id.", ephemeral: true);
                return;
            }

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync($"Event {customId} not found.", ephemeral: true);
                return;
            }

            await RespondAsync("Cancelling.", ephemeral: true);

            await _embedUpdateService.UpdateEventEmbed(event_.Id, cancelled: true);

            await _storageService.CancelEventAsync(event_.Id);
        }
    }
}

