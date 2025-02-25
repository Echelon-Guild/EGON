using Discord;
using Discord.WebSocket;
using EGON.DiscordBot.Models;
using Microsoft.Extensions.Hosting;

namespace EGON.DiscordBot.Services
{
    public class ScheduledMessageService : BackgroundService
    {
        private readonly DiscordSocketClient _client;

        private readonly StorageService _storageService;

        private readonly EmbedFactory _embedFactory;

        public ScheduledMessageService(DiscordSocketClient client, StorageService storageService, EmbedFactory embedFactory)
        {
            _client = client;
            _storageService = storageService;
            _embedFactory = embedFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;

                IEnumerable<ScheduledMessage>? messages = _storageService.GetMessagesToSend();

                if (messages is null)
                {
                    return;
                }

                foreach (ScheduledMessage msg in messages)
                {
                    var user = _client.GetUser(msg.UserId);
                    if (user != null)
                    {
                        var dmChannel = await user.CreateDMChannelAsync();

                        string rowKey = msg.EventId.ToString();

                        EchelonEvent? event_ = _storageService.GetEvent(msg.EventId);

                        Embed embed = _embedFactory.CreateEventEmbed(event_, withLink: true);

                        await dmChannel.SendMessageAsync(msg.Message, embed: embed);

                        await _storageService.DeleteScheduledMessageAsync(msg);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }


    }
}

