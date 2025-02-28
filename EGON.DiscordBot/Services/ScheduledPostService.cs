using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EGON.DiscordBot.Models;
using Microsoft.Extensions.Hosting;

namespace EGON.DiscordBot.Services
{
    public class ScheduledPostService : BackgroundService
    {
        private readonly DiscordSocketClient _client;

        private readonly StorageService _storageService;

        private readonly EmbedFactory _embedFactory;

        private ulong? _guildId;

        public ScheduledPostService(DiscordSocketClient client, StorageService storageService, EmbedFactory embedFactory)
        {
            _client = client;
            _storageService = storageService;
            _embedFactory = embedFactory;

            _client.Ready += Ready;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_guildId is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }    

                IEnumerable<ScheduledPost>? posts = _storageService.GetPostsToSend();

                foreach (ScheduledPost post in posts ?? [])
                {
                    var channel = _client.GetGuild(_guildId.Value).GetTextChannel(post.ChannelId);

                    if (channel is null)
                    {
                        // TODO: Create some error logging.
                        continue;
                    }

                    EchelonEvent? event_ = _storageService.GetEvent(post.EventId);

                    if (event_ is null)
                    {
                        continue;
                    }

                    Embed embed = _embedFactory.CreateEventEmbed(event_);

                    RestUserMessage discordPost = await channel.SendMessageAsync(embed: embed);

                    event_.MessageId = discordPost.Id;

                    await _storageService.UpsertEventAsync(event_);

                    await _storageService.DeletePostAsync(post);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            
        }

        private async Task Ready()
        {
            _client.Ready -= Ready;

            _guildId = ulong.Parse(Environment.GetEnvironmentVariable("DISCORD_SERVER_ID"));
        }
    }
}
