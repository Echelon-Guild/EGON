//using Discord;
//using Discord.Interactions;
//using EGON.DiscordBot.Models;
//using EGON.DiscordBot.Services;
//using NodaTime;

//namespace EGON.DiscordBot.Modules
//{
//    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
//    {
//        private readonly StorageService _storageService;
//        private readonly BlobUploadService _blobUploadService;

//        private readonly EmbedFactory _embedFactory;

//        private int[] _longMonths = [1, 3, 5, 7, 8, 10, 12];

//        private static Dictionary<ulong, ScheduleEventRequest> _requestWorkingCache = new();

//        public ScheduleModule(StorageService storageService, BlobUploadService blobUploadService, EmbedFactory embedFactory)
//        {
//            _storageService = storageService;

//            _blobUploadService = blobUploadService;

//            _embedFactory = embedFactory;
//        }

//        private string CreateReminderMessage(EchelonEvent ecEvent)
//        {
//            return $"Reminder!\nYou have the event {ecEvent.Name} in 30 minutes!\n<t:{ecEvent.EventDateTime.ToUnixTimeSeconds()}:F>!";
//        }

//        // Cancel an event
//        [SlashCommand("cancel", "Cancel an event.")]
//        public async Task Cancel(string eventId)
//        {
//            ulong id = ulong.Parse(eventId);

//            EchelonEvent? event_ = _storageService.GetEvent(id);

//            if (event_ is null)
//            {
//                await RespondAsync($"No event found matching event id {eventId}", ephemeral: true);
//                return;
//            }

//            if (event_.OrganizerUserId != Context.User.Username)
//            {
//                await RespondAsync($"Doesn't look like that's one of your events. Only the organizer can cancel an event.", ephemeral: true);
//                return;
//            }

//            var areYouSureDropdown = new SelectMenuBuilder()
//                .WithCustomId($"cancel_event_{id}")
//                .WithPlaceholder("Are you sure?")
//                .AddOption("Yes, I'm sure", "Yes")
//                .AddOption("No, I changed my mind.", "No");

//            var builder = new ComponentBuilder().WithSelectMenu(areYouSureDropdown);

//            await RespondAsync("Are you sure?", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("cancel_event_*")]
//        public async Task HandleCancelEvent(string customId, string yesOrNo)
//        {
//            ulong eventId = ulong.Parse(customId);

//            if (yesOrNo == "Yes")
//            {
//                await RespondAsync("Event cancelled!", ephemeral: true);

//                await UpdateEventEmbed(eventId, true);

//                await _storageService.CancelEventAsync(eventId);
//            }
//            else
//            {
//                await RespondAsync("Phew. That was close.", ephemeral: true);
//            }


//        }

//        // Create a new event

//        // The below is a chain. You trigger it with a slash command then respond to the dropdowns until you reach the end
//        // I tried to keep things in order.

//        [SlashCommand("mythic", "Schedule a Mythic+")]
//        public async Task Mythic(string name, string description, IAttachment? image = null)
//        {
//            if (!_storageService.IsApprovedCaller(Context.User.Username, "mythic"))
//            {
//                await RespondAsync("You are not authorized to create dungeons! Sorry!", ephemeral: true);
//            }

//            ulong requestId = GetNextAvailableRequestId();

//            ScheduleEventRequest request = new();

//            request.Name = name;
//            request.Id = GetNextAvailableRequestId();
//            request.EventType = EventType.Dungeon;
//            request.Description = description;

//            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
//            {
//                var countryDropdown = new SelectMenuBuilder()
//                    .WithCustomId($"country_selected_{request.Id}")
//                    .WithPlaceholder("Please select your country")
//                    .AddOption("US", "US")
//                    .AddOption("Canada", "CAN")
//                    .AddOption("Australia", "AUS");

//                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

//                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
//            }
//            else
//            {
//                await RespondToTypeSelected(request.Id);
//            }


//            string imageUrl;

//            if (image is null)
//                imageUrl = "https://storeechbotpublic.blob.core.windows.net/echelon-bot-public-images/Echelon-Logo.png";
//            else
//                imageUrl = (await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images")).ToString();

//            request.ImageUrl = imageUrl;

//            _requestWorkingCache.Add(request.Id, request);
//        }

//        [SlashCommand("raid", "Schedule a Raid")]
//        public async Task Raid(string name, string description, IAttachment image)
//        {
//            if (!_storageService.IsApprovedCaller(Context.User.Username, "mythic"))
//            {
//                await RespondAsync("You are not authorized to create raids! Sorry!", ephemeral: true);
//            }

//            ulong requestId = GetNextAvailableRequestId();

//            ScheduleEventRequest request = new();

//            request.Name = name;
//            request.Id = GetNextAvailableRequestId();
//            request.EventType = EventType.Raid;
//            request.Description = description;

//            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
//            {
//                var countryDropdown = new SelectMenuBuilder()
//                    .WithCustomId($"country_selected_{request.Id}")
//                    .WithPlaceholder("Please select your country")
//                    .AddOption("US", "US")
//                    .AddOption("Canada", "CAN")
//                    .AddOption("Australia", "AUS");

//                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

//                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
//            }
//            else
//            {
//                await RespondToTypeSelected(request.Id);
//            }


//            string imageUrl = (await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images")).ToString();

//            request.ImageUrl = imageUrl;

//            _requestWorkingCache.Add(request.Id, request);
//        }

//        [SlashCommand("event", "Schedule a non-WoW event")]
//        public async Task Event(string name, string description, IAttachment? image = null)
//        {
//            if (!_storageService.IsApprovedCaller(Context.User.Username, "event"))
//            {
//                await RespondAsync("You are not authorized to create events! Sorry!", ephemeral: true);
//            }

//            ulong requestId = GetNextAvailableRequestId();

//            ScheduleEventRequest request = new();

//            request.Name = name;
//            request.Id = GetNextAvailableRequestId();
//            request.EventType = EventType.Event;
//            request.Description = description;

//            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
//            {
//                var countryDropdown = new SelectMenuBuilder()
//                    .WithCustomId($"country_selected_{request.Id}")
//                    .WithPlaceholder("Please select your country")
//                    .AddOption("US", "US")
//                    .AddOption("Canada", "CAN")
//                    .AddOption("Australia", "AUS");

//                var builder = new ComponentBuilder().WithSelectMenu(countryDropdown);

//                await RespondAsync("Please select your country!", components: builder.Build(), ephemeral: true);
//            }
//            else
//            {
//                await RespondToTypeSelected(request.Id);
//            }


//            string imageUrl;

//            if (image is null)
//                imageUrl = "https://storeechbotpublic.blob.core.windows.net/echelon-bot-public-images/Echelon-Logo.png";
//            else
//                imageUrl = (await _blobUploadService.UploadBlobAsync(image, "echelon-bot-public-images")).ToString();

//            request.ImageUrl = imageUrl;

//            _requestWorkingCache.Add(request.Id, request);
//        }

//        [ComponentInteraction("country_selected_*")]
//        public async Task HandleCountrySelected(string customId, string selectedCountry)
//        {
//            ulong requestId = ulong.Parse(customId);

//            var timeZoneSelectMenu = new SelectMenuBuilder()
//                .WithCustomId($"timezone_selected_{requestId}")
//                .WithPlaceholder("Please select your time zone");

//            if (selectedCountry == "US")
//            {
//                timeZoneSelectMenu.AddOption("America/New_York", "America/New_York");
//                timeZoneSelectMenu.AddOption("America/Chicago", "America/Chicago");
//                timeZoneSelectMenu.AddOption("America/Denver", "America/Denver");
//                timeZoneSelectMenu.AddOption("America/Los_Angeles", "America/Los_Angeles");
//                timeZoneSelectMenu.AddOption("America/Anchorage", "America/Anchorage");
//                timeZoneSelectMenu.AddOption("America/Phoenix", "America/Phoenix");
//                timeZoneSelectMenu.AddOption("America/Detroit", "America/Detroit");
//                timeZoneSelectMenu.AddOption("America/Indiana/Indianapolis", "America/Indiana/Indianapolis");
//                timeZoneSelectMenu.AddOption("America/Indiana/Knox", "America/Indiana/Knox");
//                timeZoneSelectMenu.AddOption("America/Indiana/Marengo", "America/Indiana/Marengo");
//                timeZoneSelectMenu.AddOption("America/Indiana/Petersburg", "America/Indiana/Petersburg");
//                timeZoneSelectMenu.AddOption("America/Indiana/Tell_City", "America/Indiana/Tell_City");
//                timeZoneSelectMenu.AddOption("America/Indiana/Vevay", "America/Indiana/Vevay");
//                timeZoneSelectMenu.AddOption("America/Indiana/Vincennes", "America/Indiana/Vincennes");
//                timeZoneSelectMenu.AddOption("America/Indiana/Winamac", "America/Indiana/Winamac");
//                timeZoneSelectMenu.AddOption("America/Kentucky/Louisville", "America/Kentucky/Louisville");
//                timeZoneSelectMenu.AddOption("America/Kentucky/Monticello", "America/Kentucky/Monticello");
//                timeZoneSelectMenu.AddOption("America/North_Dakota/Beulah", "America/North_Dakota/Beulah");
//                timeZoneSelectMenu.AddOption("America/North_Dakota/Center", "America/North_Dakota/Center");
//                timeZoneSelectMenu.AddOption("America/North_Dakota/New_Salem", "America/North_Dakota/New_Salem");
//                timeZoneSelectMenu.AddOption("Pacific/Honolulu", "Pacific/Honolulu");
//            }

//            if (selectedCountry == "CAN")
//            {
//                timeZoneSelectMenu.AddOption("America/Toronto", "America/Toronto");
//                timeZoneSelectMenu.AddOption("America/Vancouver", "America/Vancouver");
//                timeZoneSelectMenu.AddOption("America/Edmonton", "America/Edmonton");
//                timeZoneSelectMenu.AddOption("America/Winnipeg", "America/Winnipeg");
//                timeZoneSelectMenu.AddOption("America/Halifax", "America/Halifax");
//                timeZoneSelectMenu.AddOption("America/St_Johns", "America/St_Johns");
//                timeZoneSelectMenu.AddOption("America/Regina", "America/Regina");
//                timeZoneSelectMenu.AddOption("America/Whitehorse", "America/Whitehorse");
//                timeZoneSelectMenu.AddOption("America/Dawson", "America/Dawson");
//                timeZoneSelectMenu.AddOption("America/Glace_Bay", "America/Glace_Bay");
//                timeZoneSelectMenu.AddOption("America/Goose_Bay", "America/Goose_Bay");
//                timeZoneSelectMenu.AddOption("America/Iqaluit", "America/Iqaluit");
//                timeZoneSelectMenu.AddOption("America/Moncton", "America/Moncton");
//                timeZoneSelectMenu.AddOption("America/Nipigon", "America/Nipigon");
//                timeZoneSelectMenu.AddOption("America/Pangnirtung", "America/Pangnirtung");
//                timeZoneSelectMenu.AddOption("America/Rainy_River", "America/Rainy_River");
//                timeZoneSelectMenu.AddOption("America/Rankin_Inlet", "America/Rankin_Inlet");
//                timeZoneSelectMenu.AddOption("America/Resolute", "America/Resolute");
//                timeZoneSelectMenu.AddOption("America/Swift_Current", "America/Swift_Current");
//                timeZoneSelectMenu.AddOption("America/Thunder_Bay", "America/Thunder_Bay");
//                timeZoneSelectMenu.AddOption("America/Yellowknife", "America/Yellowknife");
//            }

//            if (selectedCountry == "AUS")
//            {
//                timeZoneSelectMenu.AddOption("Australia/Sydney", "Australia/Sydney");
//                timeZoneSelectMenu.AddOption("Australia/Melbourne", "Australia/Melbourne");
//                timeZoneSelectMenu.AddOption("Australia/Brisbane", "Australia/Brisbane");
//                timeZoneSelectMenu.AddOption("Australia/Perth", "Australia/Perth");
//                timeZoneSelectMenu.AddOption("Australia/Adelaide", "Australia/Adelaide");
//                timeZoneSelectMenu.AddOption("Australia/Hobart", "Australia/Hobart");
//                timeZoneSelectMenu.AddOption("Australia/Darwin", "Australia/Darwin");
//                timeZoneSelectMenu.AddOption("Australia/Broken_Hill", "Australia/Broken_Hill");
//                timeZoneSelectMenu.AddOption("Australia/Lindeman", "Australia/Lindeman");
//                timeZoneSelectMenu.AddOption("Australia/Lord_Howe", "Australia/Lord_Howe");
//            }

//            var builder = new ComponentBuilder().WithSelectMenu(timeZoneSelectMenu);

//            await RespondAsync("Please select your time zone", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("timezone_selected_*")]
//        public async Task HandleTimeZoneSelected(string customId, string selectedTimeZone)
//        {
//            ulong requestId = ulong.Parse(customId);

//            EchelonUser? user = _storageService.GetUser(Context.User.Username);

//            if (user is null)
//            {
//                user = new EchelonUser()
//                {
//                    DiscordDisplayName = Context.User.GlobalName,
//                    DiscordName = Context.User.Username,
//                    TimeZone = selectedTimeZone
//                };
//            }
//            else
//            {
//                user.TimeZone = selectedTimeZone;
//            }

//            await _storageService.UpsertUserAsync(user);

//            await RespondToTypeSelected(requestId);
//        }

//        private async Task RespondToTypeSelected(ulong requestId)
//        {
//            var monthDropdown = new SelectMenuBuilder()
//                .WithCustomId($"month_select_{requestId}")
//                .WithPlaceholder("Select the month of the event")
//                .AddOption(DateTime.Now.ToString("MMMM"), DateTime.Now.Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(1).ToString("MMMM"), DateTime.Now.AddMonths(1).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(2).ToString("MMMM"), DateTime.Now.AddMonths(2).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(3).ToString("MMMM"), DateTime.Now.AddMonths(3).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(4).ToString("MMMM"), DateTime.Now.AddMonths(4).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(5).ToString("MMMM"), DateTime.Now.AddMonths(5).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(6).ToString("MMMM"), DateTime.Now.AddMonths(6).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(7).ToString("MMMM"), DateTime.Now.AddMonths(7).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(8).ToString("MMMM"), DateTime.Now.AddMonths(8).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(9).ToString("MMMM"), DateTime.Now.AddMonths(9).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(10).ToString("MMMM"), DateTime.Now.AddMonths(10).Month.ToString())
//                .AddOption(DateTime.Now.AddMonths(11).ToString("MMMM"), DateTime.Now.AddMonths(11).Month.ToString());

//            var builder = new ComponentBuilder().WithSelectMenu(monthDropdown);

//            await RespondAsync("Select the month of the event:", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("month_select_*")]
//        public async Task HandleMonthSelected(string customId, string month)
//        {
//            ulong requestId = ulong.Parse(customId);

//            int _month = int.Parse(month);

//            _requestWorkingCache[requestId].Month = _month;

//            var weekDropdown = new SelectMenuBuilder()
//                .WithCustomId($"week_select_{requestId}")
//                .WithPlaceholder("Select the week of the event");

//            AddWeekOptions(weekDropdown, _month);

//            var builder = new ComponentBuilder().WithSelectMenu(weekDropdown);

//            await RespondAsync("Select the week of the event:", components: builder.Build(), ephemeral: true);
//        }
//        private void AddWeekOptions(SelectMenuBuilder builder, int month)
//        {
//            builder.AddOption("Day 1-7", "1")
//                .AddOption("Day 8-14", "2")
//                .AddOption("Day 15-21", "3")
//                .AddOption("Day 22-28", "4");

//            if (_longMonths.Contains(month))
//            {
//                builder.AddOption("Day 29-31", "5");
//            }
//            else if (month != 2)
//            {
//                //TODO: Handle leap year
//                builder.AddOption("Day 29-30", "5");
//            }
//        }

//        [ComponentInteraction("week_select_*")]
//        public async Task HandleWeekSelected(string customId, string week)
//        {
//            ulong requestId = ulong.Parse(customId);
//            int _week = int.Parse(week);

//            _requestWorkingCache[requestId].Week = _week;

//            var dayDropdown = new SelectMenuBuilder()
//                .WithCustomId($"day_select_{requestId}")
//                .WithPlaceholder("Select the day of the event");

//            AddDayOptions(dayDropdown, _requestWorkingCache[requestId].Month, _week);

//            var builder = new ComponentBuilder().WithSelectMenu(dayDropdown);

//            await RespondAsync("Select the day of the event:", components: builder.Build(), ephemeral: true);
//        }

//        private void AddDayOptions(SelectMenuBuilder builder, int month, int week)
//        {
//            int startingDay = 0;

//            if (week == 1)
//            {
//                startingDay = 1;
//            }
//            else if (week == 2)
//            {
//                startingDay = 8;
//            }
//            else if (week == 3)
//            {
//                startingDay = 15;
//            }
//            else if (week == 4)
//            {
//                startingDay = 22;
//            }
//            else if (week == 5)
//            {
//                startingDay = 29;
//            }
//            else
//            {
//                throw new Exception("Something went horribly wrong. You got a week number that isn't 1-5");
//            }

//            int numberOfDaysInWeek = 7;

//            if (week == 5)
//            {
//                if (_longMonths.Contains(month))
//                {
//                    numberOfDaysInWeek = 3;
//                }
//                else
//                {
//                    numberOfDaysInWeek = 2;
//                }

//                //TODO: If leap year, numberOfDaysInWeek is 1.
//            }

//            for (int i = 0; i < numberOfDaysInWeek; i++)
//            {
//                int day = startingDay + i;

//                int year = DateTime.Now.Year;

//                if (month < DateTime.Now.Month)
//                    ++year;

//                DateTime date = new DateTime(year, month, day);

//                string dayString = date.ToString("dddd, MMMM dd");

//                builder.AddOption(dayString, day.ToString());
//            }
//        }

//        [ComponentInteraction("day_select_*")]
//        public async Task HandleDaySelected(string customId, string day)
//        {
//            ulong requestId = ulong.Parse(customId);
//            int _day = int.Parse(day);

//            _requestWorkingCache[requestId].Day = _day;

//            var hourDropdown = new SelectMenuBuilder()
//                .WithCustomId($"hour_select_{requestId}")
//                .WithPlaceholder("Select the hour of the event");

//            AddHourOptions(hourDropdown);

//            var builder = new ComponentBuilder().WithSelectMenu(hourDropdown);

//            await RespondAsync("Select the hour of the event:", components: builder.Build(), ephemeral: true);
//        }

//        private void AddHourOptions(SelectMenuBuilder builder)
//        {
//            builder.AddOption("12:00 AM", "0");
//            builder.AddOption("1:00 AM", "1");
//            builder.AddOption("2:00 AM", "2");
//            builder.AddOption("3:00 AM", "3");
//            builder.AddOption("4:00 AM", "4");
//            builder.AddOption("5:00 AM", "5");
//            builder.AddOption("6:00 AM", "6");
//            builder.AddOption("7:00 AM", "7");
//            builder.AddOption("8:00 AM", "8");
//            builder.AddOption("9:00 AM", "9");
//            builder.AddOption("10:00 AM", "10");
//            builder.AddOption("11:00 AM", "11");
//            builder.AddOption("12:00 PM", "12");
//            builder.AddOption("1:00 PM", "13");
//            builder.AddOption("2:00 PM", "14");
//            builder.AddOption("3:00 PM", "15");
//            builder.AddOption("4:00 PM", "16");
//            builder.AddOption("5:00 PM", "17");
//            builder.AddOption("6:00 PM", "18");
//            builder.AddOption("7:00 PM", "19");
//            builder.AddOption("8:00 PM", "20");
//            builder.AddOption("9:00 PM", "21");
//            builder.AddOption("10:00 PM", "22");
//            builder.AddOption("11:00 PM", "23");
//        }

//        [ComponentInteraction("hour_select_*")]
//        public async Task HandleHourSelected(string customId, string hour)
//        {
//            ulong requestId = ulong.Parse(customId);
//            int _hour = int.Parse(hour);

//            _requestWorkingCache[requestId].Hour = _hour;

//            var minuteDropdown = new SelectMenuBuilder()
//                .WithCustomId($"minute_select_{requestId}")
//                .WithPlaceholder("Select the minute of the event")
//                .AddOption("00", "00")
//                .AddOption("15", "15")
//                .AddOption("30", "30")
//                .AddOption("45", "45");

//            var builder = new ComponentBuilder().WithSelectMenu(minuteDropdown);

//            await RespondAsync("Select the minute of the event:", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("minute_select_*")]
//        public async Task HandleMinuteSelected(string customId, string minute)
//        {
//            ulong requestId = ulong.Parse(customId);
//            int _minute = int.Parse(minute);

//            _requestWorkingCache[requestId].Minute = _minute;

//            ScheduleEventRequest scheduleEventRequest = _requestWorkingCache[requestId];

//            await RespondToEventRequestAsync(scheduleEventRequest);

//            _requestWorkingCache.Remove(scheduleEventRequest.Id);
//        }

//        private async Task RespondToEventRequestAsync(ScheduleEventRequest scheduleEventRequest)
//        {
//            await DeferAsync();

//            if (scheduleEventRequest.Year is null)
//            {
//                scheduleEventRequest.Year = DateTime.Now.Year;

//                if (scheduleEventRequest.Month < DateTime.Now.Month)
//                    ++scheduleEventRequest.Year;
//            }

//            EchelonUser? organizer = _storageService.GetUser(Context.User.Username);

//            if (organizer is null) { organizer = new() { TimeZone = "America/New_York" }; }

//            DateTimeZone tz = DateTimeZoneProviders.Tzdb[organizer.TimeZone];

//            // 2. Create a LocalDateTime (no offset yet)
//            var localDateTime = new LocalDateTime(scheduleEventRequest.Year.Value,
//                                                  scheduleEventRequest.Month,
//                                                  scheduleEventRequest.Day,
//                                                  scheduleEventRequest.Hour,
//                                                  scheduleEventRequest.Minute, 0);

//            var zonedDateTime = tz.AtStrictly(localDateTime);

//            var offsetDateTime = zonedDateTime.ToOffsetDateTime();

//            DateTimeOffset eventDateTime = offsetDateTime.ToDateTimeOffset();

//            EchelonEvent event_ = new()
//            {
//                Id = scheduleEventRequest.Id,
//                Name = scheduleEventRequest.Name,
//                Description = scheduleEventRequest.Description,
//                Organizer = Context.User.GlobalName,
//                OrganizerUserId = Context.User.Username,
//                ImageUrl = scheduleEventRequest.ImageUrl,
//                Footer = _embedFactory.GetRandomFooter(),
//                EventDateTime = eventDateTime,
//                EventType = scheduleEventRequest.EventType.Value
//            };

//            IUserMessage message;

//            if (scheduleEventRequest.EventType == EventType.Event)
//                message = await RespondToMeetingEventAsync(event_);
//            else
//                message = await RespondToGameEventAsync(event_);

//            event_.MessageId = message.Id;

//            event_.MessageUrl = message.GetJumpUrl();

//            await _storageService.UpsertEventAsync(event_);
//        }

//        private async Task<IUserMessage> RespondToGameEventAsync(EchelonEvent ecEvent)
//        {
//            MessageComponent components = new ComponentBuilder()
//                .WithButton("Sign Up", $"signup_event_{ecEvent.Id}", row: 0)
//                .WithButton("Reset Spec", $"reset_class_{ecEvent.Id}", row: 0, style: ButtonStyle.Danger)
//                .WithButton("Absence", $"absence_event_{ecEvent.Id}", row: 1, style: ButtonStyle.Secondary)
//                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}", row: 1, style: ButtonStyle.Secondary)
//                .WithButton("Late", $"late_event_{ecEvent.Id}", row: 1, style: ButtonStyle.Secondary)
//                .Build();

//            Embed embed = _embedFactory.CreateEventEmbed(ecEvent);

//            var message = await FollowupAsync(embed: embed, components: components); // Sends the actual message
//            return message;
//        }

//        private async Task<IUserMessage> RespondToMeetingEventAsync(EchelonEvent ecEvent)
//        {
//            MessageComponent components = new ComponentBuilder()
//                .WithButton("Sign Up", $"signupmeeting_{ecEvent.Id}", row: 0)
//                .WithButton("Absence", $"absence_event_{ecEvent.Id}", row: 0, style: ButtonStyle.Secondary)
//                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}", row: 1, style: ButtonStyle.Secondary)
//                .WithButton("Late", $"late_event_{ecEvent.Id}", row: 1, style: ButtonStyle.Secondary)
//                .Build();

//            Embed embed = _embedFactory.CreateEventEmbed(ecEvent);

//            var message = await FollowupAsync(embed: embed, components: components);
//            return message;
//        }

//        private ulong GetNextAvailableRequestId()
//        {
//            return (ulong)Random.Shared.Next();
//        }

//        public async Task UpdateEventEmbed(ulong eventId, bool cancelled = false)
//        {
//            // Retrieve event entity (including MessageId)
//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                return;
//            }

//            // Retrieve the Discord message
//            var channel = Context.Client.GetChannel(Context.Channel.Id) as IMessageChannel;
//            var message = await channel.GetMessageAsync(event_.MessageId) as IUserMessage;

//            if (message is null)
//            {
//                return;
//            }

//            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

//            Embed? embed;

//            if (cancelled)
//                embed = _embedFactory.CreateCancelledEventEmbed(event_);
//            else
//                embed = _embedFactory.CreateEventEmbed(event_, attendees);

//            // Modify the existing message with the updated embed
//            await message.ModifyAsync(msg =>
//            {
//                msg.Embed = embed;

//                if (cancelled)
//                    msg.Components = new ComponentBuilder().Build();
//            });
//        }

//        // Record response to a meeting or game event.

//        [ComponentInteraction("signupmeeting_*")]
//        public async Task HandleMeetingSignup(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            AttendeeRecord record = new()
//            {
//                Id = GetNextAvailableAttendeeRecordId(),
//                EventId = eventId,
//                DiscordDisplayName = Context.User.GlobalName,
//                DiscordName = Context.User.Username,
//                Role = "Attendee"
//            };

//            await _storageService.UpsertAttendeeAsync(record);

//            await UpdateEventEmbed(eventId);

//            ScheduledMessage message = new()
//            {
//                EventId = eventId,
//                Message = CreateReminderMessage(event_),
//                SendTime = event_.EventDateTime.AddMinutes(-30),
//                UserId = Context.User.Id,
//                EventUrl = event_.MessageUrl
//            };

//            await _storageService.UpsertScheduledMessageAsync(message);

//            await RespondAsync("See you at the meeting!", ephemeral: true);
//        }

//        [ComponentInteraction("absence_event_*")]
//        public async Task HandleAbscence(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            AttendeeRecord record = new()
//            {
//                Id = GetNextAvailableAttendeeRecordId(),
//                EventId = eventId,
//                DiscordDisplayName = Context.User.GlobalName,
//                DiscordName = Context.User.Username,
//                Role = "Absent"
//            };

//            await _storageService.UpsertAttendeeAsync(record);

//            await UpdateEventEmbed(eventId);

//            await RespondAsync("We'll miss you! Thank you for responding!", ephemeral: true);

//            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

//            if (messages is null)
//            {
//                return;
//            }

//            foreach (ScheduledMessage message in messages)
//            {
//                await _storageService.DeleteScheduledMessageAsync(message);
//            }
//        }

//        [ComponentInteraction("tentative_event_*")]
//        public async Task HandleTentative(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            AttendeeRecord record = new()
//            {
//                Id = GetNextAvailableAttendeeRecordId(),
//                EventId = eventId,
//                DiscordDisplayName = Context.User.GlobalName,
//                DiscordName = Context.User.Username,
//                Role = "Tentative"
//            };

//            await _storageService.UpsertAttendeeAsync(record);

//            await UpdateEventEmbed(eventId);

//            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

//            if (messages is null || !messages.Any())
//            {
//                ScheduledMessage message = new()
//                {
//                    EventId = eventId,
//                    Message = CreateReminderMessage(event_),
//                    SendTime = event_.EventDateTime.AddMinutes(-30),
//                    UserId = Context.User.Id,
//                    EventUrl = event_.MessageUrl
//                };

//                await _storageService.UpsertScheduledMessageAsync(message);
//            }

//            await RespondAsync("We hope to see you! Thank you for responding!", ephemeral: true);
//        }

//        [ComponentInteraction("late_event_*")]
//        public async Task HandleLate(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            var minutesLateDropdown = new SelectMenuBuilder()
//                .WithCustomId($"handle_minutes_late_{eventId}")
//                .WithPlaceholder("About how late do you think?")
//                .AddOption("15 minutes", "15")
//                .AddOption("30 minutes", "30")
//                .AddOption("45 minutes", "45")
//                .AddOption("Hour or more", "60");

//            var builder = new ComponentBuilder().WithSelectMenu(minutesLateDropdown);

//            await RespondAsync("About how late do you think?", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("handle_minutes_late_*")]
//        public async Task HandleMinutesLate(string customId, string minutesLate)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            AttendeeRecord record = new()
//            {
//                Id = GetNextAvailableAttendeeRecordId(),
//                EventId = eventId,
//                DiscordDisplayName = Context.User.GlobalName,
//                DiscordName = Context.User.Username,
//                Role = "Late",
//                MinutesLate = minutesLate
//            };

//            await _storageService.UpsertAttendeeAsync(record);
//            await UpdateEventEmbed(eventId);

//            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

//            if (messages is null || !messages.Any())
//            {
//                ScheduledMessage message = new()
//                {
//                    EventId = eventId,
//                    Message = CreateReminderMessage(event_),
//                    SendTime = event_.EventDateTime.AddMinutes(-30),
//                    UserId = Context.User.Id,
//                    EventUrl = event_.MessageUrl
//                };

//                await _storageService.UpsertScheduledMessageAsync(message);
//            }

//            await RespondAsync("Thank you for letting us know!", ephemeral: true);
//        }

//        // Game event signup is a bit more complicated, so here's it's section.
//        [ComponentInteraction("signup_event_*")]
//        public async Task HandleSignup(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            if (event_ is null)
//            {
//                await RespondAsync("This event isn't in the database. It was probably deleted. You can't respond to it.", ephemeral: true);
//                return;
//            }

//            if (_storageService.IsUserRegisteredToSignUpToWoWEvents(Context.User.Username))
//            {
//                EchelonUser? user = _storageService.GetUser(Context.User.Username);

//                if (user is null)
//                {
//                    await RespondAsync("Tell Chris something weird happened in ScheduleModule on line 750.");
//                    return;
//                }

//                var role = GetRole(user.Class, user.Spec);

//                AttendeeRecord record = new()
//                {
//                    Id = GetNextAvailableAttendeeRecordId(),
//                    EventId = eventId,
//                    DiscordDisplayName = Context.User.GlobalName,
//                    DiscordName = Context.User.Username,
//                    Role = role,
//                    Class = user.Class,
//                    Spec = user.Spec
//                };


//                await _storageService.UpsertAttendeeAsync(record);

//                await UpdateEventEmbed(eventId);

//                await RespondAsync($"✅ {Context.User.GlobalName} signed up as a **{record.Spec.Prettyfy().ToUpper()} {record.Class.Prettyfy().ToUpper()}** ({record.Role})", ephemeral: true);

//                ScheduledMessage message = new()
//                {
//                    EventId = eventId,
//                    Message = CreateReminderMessage(event_),
//                    SendTime = event_.EventDateTime.AddMinutes(-30),
//                    UserId = Context.User.Id,
//                    EventUrl = event_.MessageUrl
//                };

//                await _storageService.UpsertScheduledMessageAsync(message);

//                return;
//            }

//            var classDropdown = new SelectMenuBuilder()
//                .WithCustomId($"class_select_{eventId}")
//                .WithPlaceholder("Select your Class")
//                .AddOption("Death Knight", "death_knight")
//                .AddOption("Demon Hunter", "demon_hunter")
//                .AddOption("Druid", "druid")
//                .AddOption("Evoker", "evoker")
//                .AddOption("Hunter", "hunter")
//                .AddOption("Mage", "mage")
//                .AddOption("Monk", "monk")
//                .AddOption("Paladin", "paladin")
//                .AddOption("Priest", "priest")
//                .AddOption("Rogue", "rogue")
//                .AddOption("Shaman", "shaman")
//                .AddOption("Warlock", "warlock")
//                .AddOption("Warrior", "warrior");

//            var builder = new ComponentBuilder().WithSelectMenu(classDropdown);

//            await RespondAsync("Select your **Class**:", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("class_select_*")]
//        public async Task HandleClassSelection(string customId, string selectedClass)
//        {
//            int eventId = int.Parse(customId);

//            var specDropdown = new SelectMenuBuilder()
//                .WithCustomId($"spec_select_{eventId}_{selectedClass}")
//                .WithPlaceholder("Select your Specialization");

//            // Add relevant specs based on selected class
//            switch (selectedClass)
//            {
//                case "death_knight":
//                    specDropdown.AddOption("Blood", "blood")
//                                .AddOption("Frost", "frost")
//                                .AddOption("Unholy", "unholy");
//                    break;
//                case "demon_hunter":
//                    specDropdown.AddOption("Havoc", "havoc")
//                                .AddOption("Vengeance", "vengeance");
//                    break;
//                case "druid":
//                    specDropdown.AddOption("Balance", "balance")
//                                .AddOption("Feral", "feral")
//                                .AddOption("Guardian", "guardian")
//                                .AddOption("Restoration", "restoration");
//                    break;
//                case "evoker":
//                    specDropdown.AddOption("Devastation", "devastation")
//                                .AddOption("Preservation", "preservation")
//                                .AddOption("Augmentation", "augmentation");
//                    break;
//                case "hunter":
//                    specDropdown.AddOption("Beast Mastery", "beast_mastery")
//                                .AddOption("Marksmanship", "marksmanship")
//                                .AddOption("Survival", "survival");
//                    break;
//                case "mage":
//                    specDropdown.AddOption("Arcane", "arcane")
//                                .AddOption("Fire", "fire")
//                                .AddOption("Frost", "frost");
//                    break;
//                case "monk":
//                    specDropdown.AddOption("Brewmaster", "brewmaster")
//                                .AddOption("Mistweaver", "mistweaver")
//                                .AddOption("Windwalker", "windwalker");
//                    break;
//                case "paladin":
//                    specDropdown.AddOption("Holy", "holy")
//                                .AddOption("Protection", "protection")
//                                .AddOption("Retribution", "retribution");
//                    break;
//                case "priest":
//                    specDropdown.AddOption("Discipline", "discipline")
//                                .AddOption("Holy", "holy")
//                                .AddOption("Shadow", "shadow");
//                    break;
//                case "rogue":
//                    specDropdown.AddOption("Assassination", "assassination")
//                                .AddOption("Outlaw", "outlaw")
//                                .AddOption("Subtlety", "subtlety");
//                    break;
//                case "shaman":
//                    specDropdown.AddOption("Elemental", "elemental")
//                                .AddOption("Enhancement", "enhancement")
//                                .AddOption("Restoration", "restoration");
//                    break;
//                case "warlock":
//                    specDropdown.AddOption("Affliction", "affliction")
//                                .AddOption("Demonology", "demonology")
//                                .AddOption("Destruction", "destruction");
//                    break;
//                case "warrior":
//                    specDropdown.AddOption("Arms", "arms")
//                                .AddOption("Fury", "fury")
//                                .AddOption("Protection", "protection");
//                    break;
//            }


//            var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

//            await RespondAsync($"You selected **{selectedClass.ToUpper()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
//        }

//        [ComponentInteraction("spec_select_*_*")]
//        public async Task HandleSpecSelection(string customId, string selectedClass, string selectedSpec)
//        {
//            if (string.IsNullOrWhiteSpace(selectedSpec))
//            {
//                await RespondAsync("❌ No specialization selected.", ephemeral: true);
//                return;
//            }

//            ulong eventId = ulong.Parse(customId);

//            var role = GetRole(selectedClass, selectedSpec);

//            AttendeeRecord record = new()
//            {
//                Id = GetNextAvailableAttendeeRecordId(),
//                EventId = eventId,
//                DiscordDisplayName = Context.User.GlobalName,
//                DiscordName = Context.User.Username,
//                Role = role,
//                Class = selectedClass,
//                Spec = selectedSpec
//            };


//            await _storageService.UpsertAttendeeAsync(record);

//            EchelonUser? user = _storageService.GetUser(Context.User.Username);

//            if (user is null)
//            {
//                user = new EchelonUser()
//                {
//                    DiscordDisplayName = Context.User.GlobalName,
//                    DiscordName = Context.User.Username,
//                    Class = selectedClass,
//                    Spec = selectedSpec
//                };
//            }
//            else
//            {
//                user.Class = selectedClass;
//                user.Spec = selectedSpec;
//            }

//            await _storageService.UpsertUserAsync(user);

//            await UpdateEventEmbed(eventId);

//            // Confirm signup
//            await RespondAsync($"✅ {Context.User.GlobalName} signed up as a **{record.Spec.Prettyfy().ToUpper()} {record.Class.Prettyfy().ToUpper()}** ({record.Role})", ephemeral: true);

//            EchelonEvent? event_ = _storageService.GetEvent(eventId);

//            // Schedule reminder
//            ScheduledMessage message = new()
//            {
//                EventId = eventId,
//                Message = CreateReminderMessage(event_),
//                SendTime = event_.EventDateTime.AddMinutes(-30),
//                UserId = Context.User.Id,
//                EventUrl = event_.MessageUrl
//            };

//            await _storageService.UpsertScheduledMessageAsync(message);
//        }

//        private string GetRole(string playerClass, string spec)
//        {
//            var tanks = new HashSet<string> { "Blood Death Knight", "Guardian Druid", "Brewmaster Monk", "Protection Paladin", "Protection Warrior", "Vengeance Demon Hunter" };
//            var healers = new HashSet<string> { "Restoration Druid", "Mistweaver Monk", "Holy Paladin", "Holy Priest", "Discipline Priest", "Restoration Shaman", "Preservation Evoker" };
//            var mDps = new HashSet<string>
//            {
//                "Assassination Rogue",
//                "Outlaw Rogue",
//                "Subtlety Rogue",
//                "Fury Warrior",
//                "Arms Warrior",
//                "Retribution Paladin",
//                "Frost Death Knight",
//                "Unholy Death Knight",
//                "Enhancement Shaman",
//                "Feral Druid",
//                "Havoc Demon Hunter",
//                "Windwalker Monk",
//                "Survival Hunter"
//            };

//            string fullSpec = $"{spec.Prettyfy()} {playerClass.Prettyfy()}";

//            if (tanks.Contains(fullSpec)) return "Tank";
//            if (healers.Contains(fullSpec)) return "Healer";
//            if (mDps.Contains(fullSpec)) return "Melee DPS";
//            return "Ranged DPS";
//        }

//        private int GetNextAvailableAttendeeRecordId()
//        {
//            return Random.Shared.Next();

//        }

//        [ComponentInteraction("reset_class_*")]
//        public async Task HandleResetClass(string customId)
//        {
//            ulong eventId = ulong.Parse(customId);

//            EchelonUser? user = _storageService.GetUser(Context.User.Username);

//            if (user is null)
//            {
//                await RespondAsync("You don't have a spec saved! Just sign up and we'll save your spec.", ephemeral: true);
//                return;
//            }

//            user.Class = string.Empty;
//            user.Spec = string.Empty;

//            await _storageService.UpsertUserAsync(user);

//            IEnumerable<AttendeeRecord>? attendees = _storageService.GetAttendeeRecords(eventId);

//            int attendeeCount = attendees.Count();

//            IEnumerable<AttendeeRecord>? records = attendees?.Where(e => e.DiscordName == Context.User.Username);

//            int recordCount = records.Count();

//            if (records is null)
//            {
//                await RespondAsync("Got it. Just sign up and we'll save your new preference!", ephemeral: true);
//                return;
//            }

//            foreach (AttendeeRecord record in records)
//            {
//                await _storageService.DeleteAttendeeRecordAsync(record);
//            }

//            IEnumerable<ScheduledMessage>? messages = _storageService.GetScheduledMessages(eventId, Context.User.Id);

//            if (messages is not null)
//            {
//                foreach (ScheduledMessage message in messages)
//                {
//                    await _storageService.DeleteScheduledMessageAsync(message);
//                }
//            }

//            await UpdateEventEmbed(eventId);

//            await RespondAsync("Got it. Just sign up and we'll save your new preference!", ephemeral: true);
//        }

//        // Timezone reset
//        [SlashCommand("resettz", "Reset your stored time zone information")]
//        public async Task ResetTZ()
//        {
//            if (!_storageService.IsUserRegisteredToCreateEvents(Context.User.Username))
//            {
//                await RespondAsync("You have no time zone information currently saved. Just create an event and we'll get you registered.", ephemeral: true);
//                return;
//            }

//            EchelonUser user = _storageService.GetUser(Context.User.Username) ?? new() { DiscordDisplayName = Context.User.GlobalName, DiscordName = Context.User.Username };

//            user.TimeZone = string.Empty;

//            await _storageService.UpsertUserAsync(user);

//            await RespondAsync("Your time zone info has been cleared.", ephemeral: true);
//        }
//    }
//}