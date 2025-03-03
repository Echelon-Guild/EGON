using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Services;

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
    }
}
