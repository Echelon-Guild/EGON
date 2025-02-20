using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EGON.DiscordBot.Models.Entities;
using EGON.DiscordBot.Services;

namespace EGON.DiscordBot.Modules
{
    public class UserModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TableClient _userTable;
        private readonly TableClient _characterTable;
        private readonly EmbedFactory _embedFactory;

        private static readonly Dictionary<string, EchelonUserEntity> _workingUserCache = new();
        private static readonly Dictionary<string, WoWCharacterEntity> _workingCharacterCache = new();

        public UserModule(TableServiceClient tableServiceClient, EmbedFactory embedFactory)
        {
            _userTable = tableServiceClient.GetTableClient(TableNames.USER_TABLE_NAME);
            _userTable.CreateIfNotExists();

            _characterTable = tableServiceClient.GetTableClient(TableNames.WOW_CHARACTER_TABLE_NAME);
            _characterTable.CreateIfNotExists();

            _embedFactory = embedFactory;
        }

        [SlashCommand("register", "Register or re-register yourself to use the bot")]
        public async Task Register()
        {
            Guid id = Guid.NewGuid();

            var entity = new EchelonUserEntity()
            {
                RowKey = Context.User.Username,

                DiscordName = Context.User.Username,
                DiscordDisplayName = Context.User.GlobalName
            };

            _workingUserCache.Add(Context.User.Username, entity);

            var classDropdown = new SelectMenuBuilder()
                .WithCustomId($"default_class_select_{id}")
                .WithPlaceholder("Select your default Class")
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

            await RespondAsync("Select your default **Class**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("default_class_select_*")]
        public async Task HandleDefaultClassSelected(string customId, string selectedClass)
        {
            Guid id = Guid.Parse(customId);

            _workingUserCache[Context.User.Username].Class = selectedClass;

            var specDropdown = new SelectMenuBuilder()
                .WithCustomId($"default_spec_select_{id}")
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

            await RespondAsync($"You selected **{selectedClass.Prettyfy()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("default_spec_select_*")]
        public async Task HandleDefaultSpecSelected(string customId, string selectedSpec)
        {
            Guid id = Guid.Parse(customId);

            _workingUserCache[Context.User.Username].Spec = selectedSpec;

            var countrySelectMenu = new SelectMenuBuilder()
                .WithCustomId($"default_country_select_{id}")
                .WithPlaceholder("Choose your country")
                .AddOption("US", "US")
                .AddOption("Canada", "CAN")
                .AddOption("Australia", "AUS");

            var builder = new ComponentBuilder().WithSelectMenu(countrySelectMenu);

            await RespondAsync("Please choose your country:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("default_country_select_*")]
        public async Task HandleCountrySelected(string customId, string selectedCountry)
        {
            Guid id = Guid.Parse(customId);

            var timeZoneSelectMenu = new SelectMenuBuilder()
                .WithCustomId($"default_timezone_select_{id}")
                .WithPlaceholder("Choose your timezone");

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

            await RespondAsync("Please choose your time zone:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("default_timezone_select_*")]
        public async Task HandleTimeZoneSelected(string customId, string selectedTimeZone)
        {
            Guid id = Guid.Parse(customId);

            _workingUserCache[Context.User.Username].TimeZone = selectedTimeZone;

            await _userTable.UpsertEntityAsync(_workingUserCache[Context.User.Username]);

            _workingUserCache.Remove(Context.User.Username);

            await RespondAsync($"Nice to meet you {Context.User.GlobalName}! Your information has been stored.");
        }

        [SlashCommand("registercharacter", "Register or re-register a character on the bot.")]
        public async Task RegisterCharacter(string name, string realm)
        {
            WoWCharacterEntity? character = _characterTable.Query<WoWCharacterEntity>(e => e.CharacterName == name && e.CharacterRealm == realm).FirstOrDefault();

            if (character is not null)
            {
                if (character.RegisteredTo != Context.User.Username)
                {
                    await RespondAsync($"{name} on {realm} is not registered to you. It's currently registered to {character.RegisteredTo} You will have to ask the character's current owner to delete it first.", ephemeral: true);
                    return;
                }
            }

            Guid id = Guid.NewGuid();

            WoWCharacterEntity entity = new()
            {
                RowKey = id.ToString(),
                Id = id,
                CharacterName = name,
                CharacterRealm = realm
            };

            _workingCharacterCache.Add(id.ToString(), entity);

            var classDropdown = new SelectMenuBuilder()
                .WithCustomId($"character_class_select_{id}")
                .WithPlaceholder($"Select the class for {name}")
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

            await RespondAsync($"Select the class for {name}", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("character_class_select_*")]
        public async Task HandleRegisterCharacterClassSelected(string customId, string selectedClass)
        {
            _workingCharacterCache[customId].Class = selectedClass;

            //var specDropdown = new SelectMenuBuilder()
            //    .WithCustomId($"character_spec_select_{customId}")
            //    .WithPlaceholder($"Select the specialization for {_workingCharacterCache[customId].CharacterName}");

            string fullCustomId = $"character_spec_select_{customId}";
            string placeholderText = $"Select the specialization for {_workingCharacterCache[customId].CharacterName}";

            var specDropdown = NewSpecDropDown(fullCustomId, selectedClass, placeholderText);

            var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

            await RespondAsync($"You selected **{selectedClass.Prettyfy()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
        }

        private SelectMenuBuilder NewSpecDropDown(string customId, string selectedClass, string placeholderText)
        {
            var specDropdown = new SelectMenuBuilder()
                .WithCustomId(customId)
                .WithPlaceholder(placeholderText);

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

            return specDropdown;
        }

        [ComponentInteraction("character_spec_select_*")]
        public async Task HandleRegisterCharacterSpecSelected(string customId, string selectedSpec)
        {
            _workingCharacterCache[customId].Specialization = selectedSpec;

            var offSpecYesNoDropdown = new SelectMenuBuilder()
                .WithCustomId($"character_offspec_yesno_select_{customId}")
                .WithPlaceholder("Do you have an offspec?")
                .AddOption("Yes", "yes")
                .AddOption("No", "no");

            var builder = new ComponentBuilder().WithSelectMenu(offSpecYesNoDropdown);

            await RespondAsync("Do you have an offspec?", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("character_offspec_yesno_select_*")]
        public async Task HandleRegisterClassOffspecYesNoSelected(string customId, string offspecYesOrNo)
        {
            if (offspecYesOrNo == "no")
            {
                Embed characterEmbed = await _embedFactory.CreateCharacterEmbedAsync(_workingCharacterCache[customId]);

                await SaveCharacterAndClearCacheAsync(customId);

                await RespondAsync("Thanks! Your character has been saved.", embed: characterEmbed);
            }
            else
            {
                string fullCustomId = $"character_offspec_select_{customId}";
                string placeholderText = "Please select your off-spec";

                var specDropdown = NewSpecDropDown(fullCustomId, _workingCharacterCache[customId].Class, placeholderText);

                var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

                await RespondAsync("Please select your off-spec", components: builder.Build(), ephemeral: true);
            }
        }

        private async Task SaveCharacterAndClearCacheAsync(string id)
        {
            _workingCharacterCache[id].PartitionKey = _workingCharacterCache[id].Class;
            _workingCharacterCache[id].RowKey = id;

            await _characterTable.UpsertEntityAsync(_workingCharacterCache[id]);

            _workingCharacterCache.Remove(id);
        }

        [ComponentInteraction("character_offspec_select_*")]
        public async Task HandleRegisterClassOffspecSelected(string customId, string specSelected)
        {
            _workingCharacterCache[customId].OffSpec = specSelected;

            Embed characterEmbed = await _embedFactory.CreateCharacterEmbedAsync(_workingCharacterCache[customId]);

            await SaveCharacterAndClearCacheAsync(customId);

            await RespondAsync("Thanks! Your character has been saved.", embed: characterEmbed);
        }
    }
}
