using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.DiscordBot.Modules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;

        public TestModule(StorageService storageService)
        {
            _storageService = storageService;
        }

        [SlashCommand("scheduleevent", "Test the scheduled event process")]
        public async Task ScheduleEvent()
        {
            await RespondAsync("Scheduling!", ephemeral: true);

            var event_ = new EchelonEvent()
            {
                Description = "Test event",
                EventDateTime = DateTime.Now.AddMinutes(10),
                EventType = EventType.Raid,
                Footer = "My body!",
                Id = (ulong)Random.Shared.Next(),
                ImageUrl = Context.User.GetAvatarUrl(),
                Name = "Raid Test Event",
                Organizer = Context.User.GlobalName,
                OrganizerUserId = Context.User.Username
            };

            await _storageService.UpsertEventAsync(event_);

            var scheduledPost = new ScheduledPost()
            {
                EventId = event_.Id,
                ChannelId = 1336099941906382901,
                SendTime = DateTime.Now
            };

            await _storageService.UpsertScheduledPostAsync(scheduledPost);
        }


    }
}
