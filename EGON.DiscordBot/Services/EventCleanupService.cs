using Discord;
using Discord.WebSocket;
using EGON.DiscordBot.Models;
using Microsoft.Extensions.Hosting;

namespace EGON.DiscordBot.Services
{
    public class EventCleanupService : BackgroundService
    {
        private readonly StorageService _storageService;
        private readonly EmbedFactory _embedFactory;
        private readonly DiscordSocketClient _discordSocketClient;

        private bool _socketClientReady = false;

        public EventCleanupService(StorageService storageService, EmbedFactory embedFactory, DiscordSocketClient discordSocketClient)
        {
            _storageService = storageService;
            _embedFactory = embedFactory;
            _discordSocketClient = discordSocketClient;

            _discordSocketClient.Ready += Ready;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_socketClientReady)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                IEnumerable<EchelonEvent>? eventsToClose = _storageService.GetEventsToClose();

                if (eventsToClose is not null && eventsToClose.Any())
                {
                    List<Task> tasks = new();

                    foreach (var event_ in eventsToClose)
                    {
                        tasks.Add(UpdateEventEmbedToClosed(event_));

                        event_.Closed = true;

                        tasks.Add(_storageService.UpsertEventAsync(event_));
                    }

                    await Task.WhenAll(tasks);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        public async Task UpdateEventEmbedToClosed(EchelonEvent ecEvent)
        {

            // Retrieve the Discord message
            var channel = _discordSocketClient.GetChannel(ecEvent.ChannelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(ecEvent.MessageId) as IUserMessage;

            if (message is null)
            {
                return;
            }

            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(ecEvent.Id);

            Embed? embed = _embedFactory.CreateEventEmbed(ecEvent, attendees);

            // Modify the existing message with the updated embed
            await message.ModifyAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = new ComponentBuilder().Build();                    
            });
        }

        private async Task Ready()
        {
            _socketClientReady = true;

            _discordSocketClient.Ready -= Ready;
        }
    }
}
