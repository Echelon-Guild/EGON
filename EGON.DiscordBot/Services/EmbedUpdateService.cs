using Discord;
using Discord.WebSocket;
using EGON.DiscordBot.Models;

namespace EGON.DiscordBot.Services
{
    public class EmbedUpdateService
    {
        private readonly DiscordSocketClient _client;
        private readonly StorageService _storageService;
        private readonly EmbedFactory _embedFactory;

        public EmbedUpdateService(DiscordSocketClient client, StorageService storageService, EmbedFactory embedFactory)
        {
            _client = client;
            _storageService = storageService;
            _embedFactory = embedFactory;
        }

        public async Task UpdateEventEmbed(ulong eventId, bool cancelled = false, bool closed = false)
        {
            // Retrieve event entity (including MessageId)
            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                return;
            }

            // Retrieve the Discord message
            var channel = _client.GetChannel(event_.ChannelId) as IMessageChannel;
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

                if (cancelled || closed)
                    msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}
