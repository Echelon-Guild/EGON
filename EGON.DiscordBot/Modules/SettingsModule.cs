using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Services;
using System.Text;

namespace EGON.DiscordBot.Modules
{
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
