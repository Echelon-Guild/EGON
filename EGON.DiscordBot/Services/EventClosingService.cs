using Discord;
using Discord.WebSocket;
using EGON.DiscordBot.Models;
using Microsoft.Extensions.Hosting;

namespace EGON.DiscordBot.Services
{
    public class EventClosingService : BackgroundService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly StorageService _storageService;

        public EventClosingService(DiscordSocketClient discordSocketClient, StorageService storageService)
        {
            _discordSocketClient = discordSocketClient;
            _storageService = storageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IEnumerable<EchelonEvent>? pastEvents = _storageService.GetEventsToClose();

                if (pastEvents is null || !pastEvents.Any())
                {
                    return;
                }

                

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

    }
}
