using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
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
            private readonly BlobUploadService _blobUploadService;
            private readonly EmbedUpdateService _embedUpdateService;

            public AdminAttendModule(StorageService storageService, BlobUploadService blobUploadService, EmbedUpdateService embedUpdateService)
            {
                _storageService = storageService;
                _blobUploadService = blobUploadService;
                _embedUpdateService = embedUpdateService;
            }

            [SlashCommand("add", "Add an attendance record for a user to an event.")]
            public async Task AdminAttendAdd(string userId, string eventId, string? userName = null, string? userClass = null, string? userSpec = null)
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
                            string.IsNullOrWhiteSpace(userSpec)  ||
                            string.IsNullOrWhiteSpace(userName))
                        {
                            await RespondAsync($"User {userId} is not in the database, so you must supply display name, class, and spec.", ephemeral: true);
                            return;
                        }

                        user = new EchelonUser()
                        {
                            Class = userClass.Replace(' ','_'),
                            Spec = userSpec.Replace(' ','_'),
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

        [Group("setting", "Update E.G.O.N. settings")]
        public class SettingsModuleCommands : InteractionModuleBase<SocketInteractionContext>
        {
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
        }
    }
}
