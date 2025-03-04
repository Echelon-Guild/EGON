using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Services;

namespace EGON.DiscordBot.Modules
{
    [Group("log", "Add or remove an event log")]
    public class LogModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;

        public LogModule(StorageService storageService)
        {
            _storageService = storageService;
        }

        [SlashCommand("add", "Add or update an event log")]
        public async Task AddLog(string eventId, string logUrl)
        {
            if (!ulong.TryParse(eventId, out ulong id))
            {
                await RespondAsync($"{eventId} isn't a valid event id.", ephemeral: true);
                return;
            }

            if (!Uri.TryCreate(logUrl, UriKind.Absolute, out Uri uri))
            {
                await RespondAsync($"{logUrl} isn't a valid url.", ephemeral: true);
                return;
            }

            await RespondAsync("Adding!", ephemeral: true);

            WoWEventLog? log = _storageService.GetWoWEventLog(id);

            if (log is null)
                log = new WoWEventLog();

            log.EventId = id;
            log.LogUrl = logUrl;

            await _storageService.UpsertWoWEventLogAsync(log);
        }

        [SlashCommand("delete", "Remove an event log")]
        public async Task DeleteLog(string eventId)
        {
            if (!ulong.TryParse(eventId, out ulong id))
            {
                await RespondAsync($"{eventId} isn't a valid event id.", ephemeral: true);
                return;
            }

            WoWEventLog? log = _storageService.GetWoWEventLog(id);

            if (log is null)
            {
                await RespondAsync("Couldn't find that log in the database. Good news! It's already deleted!", ephemeral: true);
                return;
            }

            await RespondAsync("Deleting!", ephemeral: true);

            await _storageService.DeleteWoWEventLog(log);


        }
    }
}
