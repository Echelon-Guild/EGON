using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using EGON.DiscordBot.Models;
using EGON.DiscordBot.Models.Modals;
using EGON.DiscordBot.Services;
using NodaTime;

namespace EGON.DiscordBot.Modules
{
    public class EventModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly StorageService _storageService;
        private readonly BlobUploadService _blobUploadService;

        private readonly EmbedUpdateService _embedUpdateService;

        private static Dictionary<ulong, NewEventRequest> _inMemoryRequests = new();

        public EventModule(StorageService storageService, BlobUploadService blobUploadService, EmbedUpdateService embedUpdateService)
        {
            _storageService = storageService;
            _blobUploadService = blobUploadService;
            _embedUpdateService = embedUpdateService;
        }

        // Creating events
        [SlashCommand("raid", "Create a new raid event")]
        public async Task Raid(IAttachment? image = null)
        {
            SocketUser? user = Context.User;

            var client = Context.Client;

            if (!_storageService.IsApprovedCaller(Context.User.Username, "raid"))
            {
                await RespondAsync("You aren't authorized to create raids! Sorry!", ephemeral: true);
                return;
            }

            ulong eventId = (ulong)Random.Shared.Next();

            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
            {
                var countryDropdown = new SelectMenuBuilder()
                    .WithCustomId($"country_selected_{eventId}")
                    .WithPlaceholder("Please select your country")
                    .AddOption("US", "US")
                    .AddOption("Canada", "CAN")
                    .AddOption("Australia", "AUS");

                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
            }
            else
            {
                await RespondWithModalAsync<NewEventModal>($"new_event_modal_{eventId}");
            }

            NewEventRequest request = new()
            {
                EventType = EventType.Raid           
            };

            if (image is null)
            {
                request.ImageUrl = _storageService.GetSetting("default-raid")?.Value ?? Context.User.GetAvatarUrl();
            }
            else
            {
                Uri blobUri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                request.ImageUrl = blobUri.OriginalString;
            }

            _inMemoryRequests.Add(eventId, request);
        }

        [SlashCommand("mythic", "Create a new mythic event")]
        public async Task Mythic(IAttachment? image = null)
        {
            if (!_storageService.IsApprovedCaller(Context.User.Username, "mythic"))
            {
                await RespondAsync("You aren't authorized to create mythic groups! Sorry!", ephemeral: true);
                return;
            }

            ulong eventId = (ulong)Random.Shared.Next();

            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
            {
                var countryDropdown = new SelectMenuBuilder()
                    .WithCustomId($"country_selected_{eventId}")
                    .WithPlaceholder("Please select your country")
                    .AddOption("US", "US")
                    .AddOption("Canada", "CAN")
                    .AddOption("Australia", "AUS");

                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
            }
            else
            {
                await RespondWithModalAsync<NewEventModal>($"new_event_modal_{eventId}");
            }

            NewEventRequest request = new()
            {
                EventType = EventType.Dungeon
            };

            if (image is null)
            {
                request.ImageUrl = _storageService.GetSetting("default-mythic")?.Value ?? Context.User.GetAvatarUrl();
            }
            else
            {
                Uri blobUri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                request.ImageUrl = blobUri.OriginalString;
            }

            _inMemoryRequests.Add(eventId, request);
        }

        [SlashCommand("event", "Create a new generic or non-WoW event")]
        public async Task Event(IAttachment? image = null)
        {
            if (!_storageService.IsApprovedCaller(Context.User.Username, "event"))
            {
                await RespondAsync("You aren't authorized to create event groups! Sorry!", ephemeral: true);
                return;
            }

            ulong eventId = (ulong)Random.Shared.Next();

            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
            {
                var countryDropdown = new SelectMenuBuilder()
                    .WithCustomId($"country_selected_{eventId}")
                    .WithPlaceholder("Please select your country")
                    .AddOption("US", "US")
                    .AddOption("Canada", "CAN")
                    .AddOption("Australia", "AUS");

                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
            }
            else
            {
                await RespondWithModalAsync<NewEventModal>($"new_event_modal_{eventId}");
            }

            NewEventRequest request = new()
            {
                EventType = EventType.Event
            };

            if (image is null)
            {
                request.ImageUrl = _storageService.GetSetting("default-event")?.Value ?? Context.User.GetAvatarUrl();
            }
            else
            {
                Uri blobUri = await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images");

                request.ImageUrl = blobUri.OriginalString;
            }

            _inMemoryRequests.Add(eventId, request);
        }

        [ModalInteraction("new_event_modal_*")]
        public async Task HandleNewEventModal(string customId, NewEventModal modal)
        {
            ulong eventId = ulong.Parse(customId);

            if (string.IsNullOrWhiteSpace(modal.Name))
            {
                await RespondAsync("You didn't provide a name. Please try again.");
                return;
            }

            if (string.IsNullOrWhiteSpace(modal.Description))
            {
                await RespondAsync("You didn't provide a description. Please try again.");
                return;
            }

            if (modal.DateTimeOfEvent == DateTime.MinValue)
            {
                await RespondAsync("You didn't provide a time. Please try again.");
                return;
            }

            string? organizerTimeZone = _storageService.GetUser(Context.User.Username)?.TimeZone;

            if (organizerTimeZone is null)
            {
                await RespondAsync("Tell Chris something weird happened in HandleNewRaidModal."); 
                return;
            }

            DateTimeZone tz = DateTimeZoneProviders.Tzdb[organizerTimeZone];

            // 2. Create a LocalDateTime (no offset yet)
            var localDateTime = new LocalDateTime(modal.DateTimeOfEvent.Year,
                                                  modal.DateTimeOfEvent.Month,
                                                  modal.DateTimeOfEvent.Day,
                                                  modal.DateTimeOfEvent.Hour,
                                                  modal.DateTimeOfEvent.Minute, 0);

            var zonedDateTime = tz.AtStrictly(localDateTime);

            var offsetDateTime = zonedDateTime.ToOffsetDateTime();

            DateTimeOffset eventDateTime = offsetDateTime.ToDateTimeOffset();

            if (eventDateTime < DateTime.UtcNow)
            {
                await RespondAsync("Can't schedule an event in the past!", ephemeral: true);
                return;
            }

            await RespondAsync("Scheduling now!", ephemeral: true);

            string userDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName;

            Footer? footer = _storageService.GetRandomFooter();

            var event_ = new EchelonEvent()
            {
                Description = modal.Description,
                EventDateTime = eventDateTime,
                EventType = _inMemoryRequests[eventId].EventType,
                Footer = footer?.Value ?? string.Empty,
                Id = eventId,
                ImageUrl = _inMemoryRequests[eventId].ImageUrl,
                Name = modal.Name,
                Organizer = Context.Guild.GetUser(Context.User.Id).DisplayName,
                OrganizerUserId = Context.User.Username
            };

            await _storageService.UpsertEventAsync(event_);

            ulong channelId;

            if (event_.EventType == EventType.Event)
            {
                channelId = Context.Channel.Id;
            }
            else if (event_.EventType == EventType.Dungeon)
            {
                channelId = ulong.Parse(Environment.GetEnvironmentVariable("MYTHIC_SIGN_UP_CHANNEL_ID") ?? throw new EnvironmentNotConfiguredException("MYTHIC_SIGN_UP_CHANNEL_ID"));
            }
            else
            {
                channelId = ulong.Parse(Environment.GetEnvironmentVariable("RAID_SIGN_UP_CHANNEL_ID") ?? throw new EnvironmentNotConfiguredException("RAID_SIGN_UP_CHANNEL_ID"));
            }

            var post = new ScheduledPost()
            {
                ChannelId = channelId,
                EventId = eventId,
                SendTime = DateTimeOffset.UtcNow
            };

            await _storageService.UpsertScheduledPostAsync(post);
        }

        [ComponentInteraction("country_selected_*")]
        public async Task HandleCountrySelected(string customId, string selectedCountry)
        {
            ulong requestId = ulong.Parse(customId);

            var timeZoneSelectMenu = new SelectMenuBuilder()
                .WithCustomId($"timezone_selected_{requestId}")
                .WithPlaceholder("Please select your time zone");

            if (selectedCountry == "US")
            {
                timeZoneSelectMenu.AddOption("America/New_York", "America/New_York");
                timeZoneSelectMenu.AddOption("America/Chicago", "America/Chicago");
                timeZoneSelectMenu.AddOption("America/Denver", "America/Denver");
                timeZoneSelectMenu.AddOption("America/Los_Angeles", "America/Los_Angeles");
                timeZoneSelectMenu.AddOption("America/Anchorage", "America/Anchorage");
                timeZoneSelectMenu.AddOption("America/Phoenix", "America/Phoenix");
                timeZoneSelectMenu.AddOption("America/Detroit", "America/Detroit");
                timeZoneSelectMenu.AddOption("America/Indiana/Indianapolis", "America/Indiana/Indianapolis");
                timeZoneSelectMenu.AddOption("America/Indiana/Knox", "America/Indiana/Knox");
                timeZoneSelectMenu.AddOption("America/Indiana/Marengo", "America/Indiana/Marengo");
                timeZoneSelectMenu.AddOption("America/Indiana/Petersburg", "America/Indiana/Petersburg");
                timeZoneSelectMenu.AddOption("America/Indiana/Tell_City", "America/Indiana/Tell_City");
                timeZoneSelectMenu.AddOption("America/Indiana/Vevay", "America/Indiana/Vevay");
                timeZoneSelectMenu.AddOption("America/Indiana/Vincennes", "America/Indiana/Vincennes");
                timeZoneSelectMenu.AddOption("America/Indiana/Winamac", "America/Indiana/Winamac");
                timeZoneSelectMenu.AddOption("America/Kentucky/Louisville", "America/Kentucky/Louisville");
                timeZoneSelectMenu.AddOption("America/Kentucky/Monticello", "America/Kentucky/Monticello");
                timeZoneSelectMenu.AddOption("America/North_Dakota/Beulah", "America/North_Dakota/Beulah");
                timeZoneSelectMenu.AddOption("America/North_Dakota/Center", "America/North_Dakota/Center");
                timeZoneSelectMenu.AddOption("America/North_Dakota/New_Salem", "America/North_Dakota/New_Salem");
                timeZoneSelectMenu.AddOption("Pacific/Honolulu", "Pacific/Honolulu");
            }

            if (selectedCountry == "CAN")
            {
                timeZoneSelectMenu.AddOption("America/Toronto", "America/Toronto");
                timeZoneSelectMenu.AddOption("America/Vancouver", "America/Vancouver");
                timeZoneSelectMenu.AddOption("America/Edmonton", "America/Edmonton");
                timeZoneSelectMenu.AddOption("America/Winnipeg", "America/Winnipeg");
                timeZoneSelectMenu.AddOption("America/Halifax", "America/Halifax");
                timeZoneSelectMenu.AddOption("America/St_Johns", "America/St_Johns");
                timeZoneSelectMenu.AddOption("America/Regina", "America/Regina");
                timeZoneSelectMenu.AddOption("America/Whitehorse", "America/Whitehorse");
                timeZoneSelectMenu.AddOption("America/Dawson", "America/Dawson");
                timeZoneSelectMenu.AddOption("America/Glace_Bay", "America/Glace_Bay");
                timeZoneSelectMenu.AddOption("America/Goose_Bay", "America/Goose_Bay");
                timeZoneSelectMenu.AddOption("America/Iqaluit", "America/Iqaluit");
                timeZoneSelectMenu.AddOption("America/Moncton", "America/Moncton");
                timeZoneSelectMenu.AddOption("America/Nipigon", "America/Nipigon");
                timeZoneSelectMenu.AddOption("America/Pangnirtung", "America/Pangnirtung");
                timeZoneSelectMenu.AddOption("America/Rainy_River", "America/Rainy_River");
                timeZoneSelectMenu.AddOption("America/Rankin_Inlet", "America/Rankin_Inlet");
                timeZoneSelectMenu.AddOption("America/Resolute", "America/Resolute");
                timeZoneSelectMenu.AddOption("America/Swift_Current", "America/Swift_Current");
                timeZoneSelectMenu.AddOption("America/Thunder_Bay", "America/Thunder_Bay");
                timeZoneSelectMenu.AddOption("America/Yellowknife", "America/Yellowknife");
            }

            if (selectedCountry == "AUS")
            {
                timeZoneSelectMenu.AddOption("Australia/Sydney", "Australia/Sydney");
                timeZoneSelectMenu.AddOption("Australia/Melbourne", "Australia/Melbourne");
                timeZoneSelectMenu.AddOption("Australia/Brisbane", "Australia/Brisbane");
                timeZoneSelectMenu.AddOption("Australia/Perth", "Australia/Perth");
                timeZoneSelectMenu.AddOption("Australia/Adelaide", "Australia/Adelaide");
                timeZoneSelectMenu.AddOption("Australia/Hobart", "Australia/Hobart");
                timeZoneSelectMenu.AddOption("Australia/Darwin", "Australia/Darwin");
                timeZoneSelectMenu.AddOption("Australia/Broken_Hill", "Australia/Broken_Hill");
                timeZoneSelectMenu.AddOption("Australia/Lindeman", "Australia/Lindeman");
                timeZoneSelectMenu.AddOption("Australia/Lord_Howe", "Australia/Lord_Howe");
            }

            var builder = new ComponentBuilder().WithSelectMenu(timeZoneSelectMenu);

            await RespondAsync("Please select your time zone", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("timezone_selected_*")]
        public async Task HandleTimeZoneSelected(string customId, string selectedTimeZone)
        {
            ulong requestId = ulong.Parse(customId);

            await RespondWithModalAsync<NewEventModal>($"new_event_modal_{requestId}");

            EchelonUser? user = _storageService.GetUser(Context.User.Username);

            if (user is null)
            {
                user = new EchelonUser()
                {
                    DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                    DiscordName = Context.User.Username,
                    TimeZone = selectedTimeZone
                };
            }
            else
            {
                user.TimeZone = selectedTimeZone;
            }

            await _storageService.UpsertUserAsync(user);
        }

        // Responding to events
        [ComponentInteraction("signupmeeting_*")]
        public async Task HandleMeetingSignup(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            await RespondAsync("See you at the meeting!", ephemeral: true);

            AttendeeRecord record = new()
            {
                Id = Random.Shared.Next(),
                EventId = eventId,
                DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                DiscordName = Context.User.Username,
                Role = "Attendee"
            };

            await _storageService.UpsertAttendeeAsync(record);

            //await UpdateEventEmbed(eventId);
            await _embedUpdateService.UpdateEventEmbed(eventId);

            ScheduledMessage message = new()
            {
                EventId = eventId,
                Message = CreateReminderMessage(event_),
                SendTime = event_.EventDateTime.AddMinutes(-30),
                UserId = Context.User.Id,
                EventUrl = event_.MessageUrl
            };

            await _storageService.UpsertScheduledMessageAsync(message);
        }

        [ComponentInteraction("absence_event_*")]
        public async Task HandleAbscence(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            await RespondAsync("We'll miss you! Thank you for responding!", ephemeral: true);

            AttendeeRecord record = new()
            {
                Id = Random.Shared.Next(),
                EventId = eventId,
                DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                DiscordName = Context.User.Username,
                Role = "Absent"
            };

            await _storageService.UpsertAttendeeAsync(record);

            //await UpdateEventEmbed(eventId);
            await _embedUpdateService.UpdateEventEmbed(eventId);

            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

            if (messages is null)
            {
                return;
            }

            foreach (ScheduledMessage message in messages)
            {
                await _storageService.DeleteScheduledMessageAsync(message);
            }
        }

        [ComponentInteraction("tentative_event_*")]
        public async Task HandleTentative(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            await RespondAsync("We hope to see you! Thank you for responding!", ephemeral: true);

            AttendeeRecord record = new()
            {
                Id = Random.Shared.Next(),
                EventId = eventId,
                DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                DiscordName = Context.User.Username,
                Role = "Tentative"
            };

            await _storageService.UpsertAttendeeAsync(record);

            //await UpdateEventEmbed(eventId);
            await _embedUpdateService.UpdateEventEmbed(eventId);

            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

            if (messages is null || !messages.Any())
            {
                ScheduledMessage message = new()
                {
                    EventId = eventId,
                    Message = CreateReminderMessage(event_),
                    SendTime = event_.EventDateTime.AddMinutes(-30),
                    UserId = Context.User.Id,
                    EventUrl = event_.MessageUrl
                };

                await _storageService.UpsertScheduledMessageAsync(message);
            }

            
        }

        [ComponentInteraction("late_event_*")]
        public async Task HandleLate(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            var minutesLateDropdown = new SelectMenuBuilder()
                .WithCustomId($"handle_minutes_late_{eventId}")
                .WithPlaceholder("About how late do you think?")
                .AddOption("15 minutes", "15")
                .AddOption("30 minutes", "30")
                .AddOption("45 minutes", "45")
                .AddOption("Hour or more", "60");

            var builder = new ComponentBuilder().WithSelectMenu(minutesLateDropdown);

            await RespondAsync("About how late do you think?", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("handle_minutes_late_*")]
        public async Task HandleMinutesLate(string customId, string minutesLate)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            await RespondAsync("Thank you for letting us know!", ephemeral: true);

            AttendeeRecord record = new()
            {
                Id = Random.Shared.Next(),
                EventId = eventId,
                DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                DiscordName = Context.User.Username,
                Role = "Late",
                MinutesLate = minutesLate
            };

            await _storageService.UpsertAttendeeAsync(record);
            await _embedUpdateService.UpdateEventEmbed(eventId);

            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

            if (messages is null || !messages.Any())
            {
                ScheduledMessage message = new()
                {
                    EventId = eventId,
                    Message = CreateReminderMessage(event_),
                    SendTime = event_.EventDateTime.AddMinutes(-30),
                    UserId = Context.User.Id,
                    EventUrl = event_.MessageUrl
                };

                await _storageService.UpsertScheduledMessageAsync(message);
            }
        }

        // Game event signup is a bit more complicated, so here's it's section.
        [ComponentInteraction("signup_event_*")]
        public async Task HandleSignup(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
                return;
            }

            if (_storageService.IsUserRegisteredToSignUpToWoWEvents(Context.User.Username))
            {
                EchelonUser? user = _storageService.GetUser(Context.User.Username);

                if (user is null)
                {
                    await RespondAsync("Tell Chris something weird happened in ScheduleModule on line 750.");
                    return;
                }

                var role = GetRole(user.Class, user.Spec);

                AttendeeRecord record = new()
                {
                    Id = Random.Shared.Next(),
                    EventId = eventId,
                    DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                    DiscordName = Context.User.Username,
                    Role = role,
                    Class = user.Class,
                    Spec = user.Spec
                };

                await RespondAsync($"✅ {Context.Guild.GetUser(Context.User.Id).DisplayName} signed up as a **{record.Spec.Prettyfy().ToUpper()} {record.Class.Prettyfy().ToUpper()}** ({record.Role})", ephemeral: true);


                await _storageService.UpsertAttendeeAsync(record);

                await _embedUpdateService.UpdateEventEmbed(eventId);

                ScheduledMessage message = new()
                {
                    EventId = eventId,
                    Message = CreateReminderMessage(event_),
                    SendTime = event_.EventDateTime.AddMinutes(-30),
                    UserId = Context.User.Id,
                    EventUrl = event_.MessageUrl
                };

                await _storageService.UpsertScheduledMessageAsync(message);

                return;
            }

            var classDropdown = new SelectMenuBuilder()
                .WithCustomId($"class_select_{eventId}")
                .WithPlaceholder("Select your Class")
                .AddOption("Death Knight", "death_knight")
                .AddOption("Demon Hunter", "demon_hunter")
                .AddOption("Druid", "druid")
                .AddOption("Evoker", "evoker")
                .AddOption("Hunter", "hunter")
                .AddOption("Mage", "mage")
                .AddOption("Monk", "monk")
                .AddOption("Paladin", "paladin")
                .AddOption("Priest", "priest")
                .AddOption("Rogue", "rogue")
                .AddOption("Shaman", "shaman")
                .AddOption("Warlock", "warlock")
                .AddOption("Warrior", "warrior");

            var builder = new ComponentBuilder().WithSelectMenu(classDropdown);

            await RespondAsync("Select your **Class**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("class_select_*")]
        public async Task HandleClassSelection(string customId, string selectedClass)
        {
            int eventId = int.Parse(customId);

            var specDropdown = new SelectMenuBuilder()
                .WithCustomId($"spec_select_{eventId}_{selectedClass}")
                .WithPlaceholder("Select your Specialization");

            // Add relevant specs based on selected class
            switch (selectedClass)
            {
                case "death_knight":
                    specDropdown.AddOption("Blood", "blood")
                                .AddOption("Frost", "frost")
                                .AddOption("Unholy", "unholy");
                    break;
                case "demon_hunter":
                    specDropdown.AddOption("Havoc", "havoc")
                                .AddOption("Vengeance", "vengeance");
                    break;
                case "druid":
                    specDropdown.AddOption("Balance", "balance")
                                .AddOption("Feral", "feral")
                                .AddOption("Guardian", "guardian")
                                .AddOption("Restoration", "restoration");
                    break;
                case "evoker":
                    specDropdown.AddOption("Devastation", "devastation")
                                .AddOption("Preservation", "preservation")
                                .AddOption("Augmentation", "augmentation");
                    break;
                case "hunter":
                    specDropdown.AddOption("Beast Mastery", "beast_mastery")
                                .AddOption("Marksmanship", "marksmanship")
                                .AddOption("Survival", "survival");
                    break;
                case "mage":
                    specDropdown.AddOption("Arcane", "arcane")
                                .AddOption("Fire", "fire")
                                .AddOption("Frost", "frost");
                    break;
                case "monk":
                    specDropdown.AddOption("Brewmaster", "brewmaster")
                                .AddOption("Mistweaver", "mistweaver")
                                .AddOption("Windwalker", "windwalker");
                    break;
                case "paladin":
                    specDropdown.AddOption("Holy", "holy")
                                .AddOption("Protection", "protection")
                                .AddOption("Retribution", "retribution");
                    break;
                case "priest":
                    specDropdown.AddOption("Discipline", "discipline")
                                .AddOption("Holy", "holy")
                                .AddOption("Shadow", "shadow");
                    break;
                case "rogue":
                    specDropdown.AddOption("Assassination", "assassination")
                                .AddOption("Outlaw", "outlaw")
                                .AddOption("Subtlety", "subtlety");
                    break;
                case "shaman":
                    specDropdown.AddOption("Elemental", "elemental")
                                .AddOption("Enhancement", "enhancement")
                                .AddOption("Restoration", "restoration");
                    break;
                case "warlock":
                    specDropdown.AddOption("Affliction", "affliction")
                                .AddOption("Demonology", "demonology")
                                .AddOption("Destruction", "destruction");
                    break;
                case "warrior":
                    specDropdown.AddOption("Arms", "arms")
                                .AddOption("Fury", "fury")
                                .AddOption("Protection", "protection");
                    break;
            }

            var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

            await RespondAsync($"You selected **{selectedClass.ToUpper()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("spec_select_*_*")]
        public async Task HandleSpecSelection(string customId, string selectedClass, string selectedSpec)
        {
            if (string.IsNullOrWhiteSpace(selectedSpec))
            {
                await RespondAsync("❌ No specialization selected.", ephemeral: true);
                return;
            }

            ulong eventId = ulong.Parse(customId);

            var role = GetRole(selectedClass, selectedSpec);

            AttendeeRecord record = new()
            {
                Id = Random.Shared.Next(),
                EventId = eventId,
                DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                DiscordName = Context.User.Username,
                Role = role,
                Class = selectedClass,
                Spec = selectedSpec
            };

            // Confirm signup

            await RespondAsync($"✅ {Context.Guild.GetUser(Context.User.Id).DisplayName} signed up as a **{record.Spec.Prettyfy().ToUpper()} {record.Class.Prettyfy().ToUpper()}** ({record.Role})", ephemeral: true);

            await _storageService.UpsertAttendeeAsync(record);

            EchelonUser? user = _storageService.GetUser(Context.User.Username);

            if (user is null)
            {
                user = new EchelonUser()
                {
                    DiscordDisplayName = Context.Guild.GetUser(Context.User.Id).DisplayName,
                    DiscordName = Context.User.Username,
                    Class = selectedClass,
                    Spec = selectedSpec
                };
            }
            else
            {
                user.Class = selectedClass;
                user.Spec = selectedSpec;
            }

            await _storageService.UpsertUserAsync(user);

            await _embedUpdateService.UpdateEventEmbed(eventId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            // Schedule reminder
            ScheduledMessage message = new()
            {
                EventId = eventId,
                Message = CreateReminderMessage(event_),
                SendTime = event_.EventDateTime.AddMinutes(-30),
                UserId = Context.User.Id,
                EventUrl = event_.MessageUrl
            };

            await _storageService.UpsertScheduledMessageAsync(message);
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

        // Cancel an event
        [SlashCommand("cancel", "Cancel an event.")]
        public async Task Cancel(string eventId)
        {
            ulong id = ulong.Parse(eventId);

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync($"No event found matching event id {eventId}", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync($"Doesn't look like that's one of your events. Only the organizer can cancel an event.", ephemeral: true);
                return;
            }

            if (event_.Closed)
            {
                await RespondAsync("Event is already closed! It can't be cancelled.", ephemeral: true);
                return;
            }

            var areYouSureDropdown = new SelectMenuBuilder()
                .WithCustomId($"cancel_event_{id}")
                .WithPlaceholder("Are you sure?")
                .AddOption("Yes, I'm sure", "Yes")
                .AddOption("No, I changed my mind.", "No");

            var builder = new ComponentBuilder().WithSelectMenu(areYouSureDropdown);

            await RespondAsync("Are you sure?", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("cancel_event_*")]
        public async Task HandleCancelEvent(string customId, string yesOrNo)
        {
            ulong eventId = ulong.Parse(customId);

            if (yesOrNo == "Yes")
            {
                await RespondAsync("Event cancelled!", ephemeral: true);

                await _embedUpdateService.UpdateEventEmbed(eventId, cancelled: true);

                await _storageService.CancelEventAsync(eventId);
            }
            else
            {
                await RespondAsync("Phew. That was close.", ephemeral: true);
            }
        }

        // Things that should probably go elsewhere
        private string CreateReminderMessage(EchelonEvent ecEvent)
        {
            return $"Reminder!\nYou are signed up for the event {ecEvent.Name}!\n<t:{ecEvent.EventDateTime.ToUnixTimeSeconds()}:F>";
        }

        [ComponentInteraction("reset_class_*")]
        public async Task HandleResetClass(string customId)
        {
            ulong eventId = ulong.Parse(customId);

            EchelonUser? user = _storageService.GetUser(Context.User.Username);

            if (user is null)
            {
                await RespondAsync("You don't have a spec saved! Just sign up and we'll save your spec.", ephemeral: true);
                return;
            }

            user.Class = string.Empty;
            user.Spec = string.Empty;

            await _storageService.UpsertUserAsync(user);

            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

            int attendeeCount = attendees.Count();

            IEnumerable<AttendeeRecord>? records = attendees?.Where(e => e.DiscordName == Context.User.Username);

            int recordCount = records.Count();

            if (records is null)
            {
                await RespondAsync("Got it. Just sign up and we'll save your new preference!", ephemeral: true);
                return;
            }

            foreach (AttendeeRecord record in records)
            {
                await _storageService.DeleteAttendeeRecordAsync(record);
            }

            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

            if (messages is not null)
            {
                foreach (ScheduledMessage message in messages)
                {
                    await _storageService.DeleteScheduledMessageAsync(message);
                }
            }

            await _embedUpdateService.UpdateEventEmbed(eventId);

            await RespondAsync("Got it. Just sign up and we'll save your new preference!", ephemeral: true);
        }

        // Timezone reset
        [SlashCommand("resettz", "Reset your stored time zone information")]
        public async Task ResetTZ()
        {
            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
            {
                await RespondAsync("You have no time zone information currently saved. Just create an event and we'll get you registered.", ephemeral: true);
                return;
            }

            EchelonUser user = _storageService.GetUser(Context.User.Username) ?? new() { DiscordDisplayName = Context.User.GlobalName, DiscordName = Context.User.Username };

            user.TimeZone = string.Empty;

            await _storageService.UpsertUserAsync(user);

            await RespondAsync("Your time zone info has been cleared.", ephemeral: true);
        }

        // Delete an event
        [SlashCommand("delete", "Delete an event.")]
        public async Task Delete(string eventId)
        {
            ulong id = ulong.Parse(eventId);

            EchelonEvent? event_ = _storageService.GetEvent(id);

            if (event_ is null)
            {
                await RespondAsync($"No event found matching event id {eventId}", ephemeral: true);
                return;
            }

            if (event_.OrganizerUserId != Context.User.Username)
            {
                await RespondAsync($"Doesn't look like that's one of your events. Only the organizer can delete an event.", ephemeral: true);
                return;
            }

            if (event_.Closed)
            {
                await RespondAsync("Event is already closed! It can't be deleted now.", ephemeral: true);
                return;
            }

            var areYouSureDropdown = new SelectMenuBuilder()
                .WithCustomId($"delete_event_{id}")
                .WithPlaceholder("Are you sure?")
                .AddOption("Yes, I'm sure", "Yes")
                .AddOption("No, I changed my mind.", "No");

            var builder = new ComponentBuilder().WithSelectMenu(areYouSureDropdown);

            await RespondAsync("Are you sure?", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("delete_event_*")]
        public async Task HandleDeleteEvent(string customId, string yesOrNo)
        {
            if (yesOrNo != "Yes")
            {
                await RespondAsync("Phew. That was close.", ephemeral: true);
                return;
            }

            ulong eventId = ulong.Parse(customId);

            EchelonEvent? event_ = _storageService.GetEvent(eventId);

            if (event_ is null)
            {
                await RespondAsync("I can't find that event! It was already deleted.", ephemeral: true);
                return;
            }

            await RespondAsync("Event deletion in progress!", ephemeral: true);

            var channel = Context.Client.GetChannel(event_.ChannelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(event_.MessageId) as IUserMessage;

            await message.DeleteAsync();

            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

            List<Task> tasks = new();

            if (attendees is not null && attendees.Any())
            {
                foreach (AttendeeRecord attendee in attendees)
                {
                    tasks.Add(_storageService.DeleteAttendeeRecordAsync(attendee));
                }
            }

            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId);

            if (messages is not null && messages.Any())
            {
                foreach (ScheduledMessage schMessage in messages)
                {
                    tasks.Add(_storageService.DeleteScheduledMessageAsync(schMessage));
                }
            }

            if (event_ is not null)
            {
                tasks.Add(_storageService.DeleteEventAsync(event_));
            }

            await Task.WhenAll(tasks);
        }
    }
}
