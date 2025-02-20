﻿using Discord;
using EGON.Library.Models;
using EGON.Library.Models.Entities;
using EGON.Library.Models.WoWApiResponse;
using EGON.Library.Services.WoW;
using EGON.Library.Utility;
using System.Text;

namespace EGON.Library.Services
{
    public class EmbedFactory
    {
        private EmoteFinder _emoteFinder;
        private WoWApiService _wowApiService;

        public EmbedFactory(EmoteFinder emoteFinder, WoWApiService woWApiService)
        {
            _emoteFinder = emoteFinder;
            _wowApiService = woWApiService;
        }

        public Embed CreateEventEmbed(EchelonEvent ecEvent, IEnumerable<AttendeeRecord> attendees = null)
        {
            Color color;

            switch (ecEvent.EventType)
            {
                case EventType.Raid:
                    {
                        color = Color.Orange;
                        break;
                    }
                case EventType.Dungeon:
                    {
                        color = Color.Green;
                        break;
                    }
                case EventType.Meeting:
                    {
                        color = Color.Blue;
                        break;
                    }
                default:
                    {
                        color = Color.Red;
                        break;
                    }
            }

            string timestamp = $"<t:{ecEvent.EventDateTime.ToUnixTimeSeconds()}:F>";

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(ecEvent.Name)
                .WithDescription(ecEvent.Description)
                .WithColor(color)
                .AddField("Scheduled Time", timestamp)
                .AddField("Event Type", ecEvent.EventType.ToString(), true)
                .AddField("Organizer", ecEvent.Organizer, true)
                .WithThumbnailUrl(ecEvent.ImageUrl)
                .WithFooter($"Powered by: {ecEvent.Footer}");

            if (attendees != null)
            {
                if (ecEvent.EventType == EventType.Meeting)
                {
                    IEnumerable<AttendeeRecord> attending = attendees.Where(e => e.Role.ToLower() == "attendee");

                    if (attending.Any())
                        embed.AddField($"✅ Attendees ({attending.Count()})", GetMeetingAttendeeString(attending));
                }
                else
                {
                    IEnumerable<AttendeeRecord> tanks = attendees.Where(e => e.Role.ToLower() == "tank");
                    IEnumerable<AttendeeRecord> healers = attendees.Where(e => e.Role.ToLower() == "healer");
                    IEnumerable<AttendeeRecord> mdps = attendees.Where(e => e.Role.ToLower() == "melee dps");
                    IEnumerable<AttendeeRecord> rdps = attendees.Where(e => e.Role.ToLower() == "ranged dps");


                    if (tanks.Any())
                        embed.AddField($"🛡️ Tanks ({tanks.Count()})", GetGameEventAttendeeString(tanks));

                    if (healers.Any())
                        embed.AddField($"❤️ Healers ({healers.Count()})", GetGameEventAttendeeString(healers));

                    if (mdps.Any())
                        embed.AddField($"🗡️ Melee DPS ({mdps.Count()})", GetGameEventAttendeeString(mdps));

                    if (rdps.Any())
                        embed.AddField($"🏹 Ranged DPS ({rdps.Count()})", GetGameEventAttendeeString(rdps));
                }

                IEnumerable<AttendeeRecord> absent = attendees.Where(e => e.Role.ToLower() == "absent");

                if (absent.Any())
                    embed.AddField($"❌ Absent ({absent.Count()})", GetMeetingAttendeeString(absent));

                IEnumerable<AttendeeRecord> tentative = attendees.Where(e => e.Role.ToLower() == "tentative");

                if (tentative.Any())
                    embed.AddField($"\U0001f9c7 Tentative ({tentative.Count()})", GetMeetingAttendeeString(tentative));
            }

            return embed.Build();
        }

        private string GetMeetingAttendeeString(IEnumerable<AttendeeRecord> attendees)
        {
            if (!attendees.Any())
                return string.Empty;

            StringBuilder sb = new();

            foreach (AttendeeRecord attendee in attendees)
            {
                sb.AppendLine($"{attendee.DiscordDisplayName}");
            }

            return sb.ToString() ?? string.Empty;
        }

        private string GetGameEventAttendeeString(IEnumerable<AttendeeRecord> attendees)
        {
            if (!attendees.Any())
                return string.Empty;

            StringBuilder sb = new();
            foreach (AttendeeRecord attendee in attendees)
            {
                sb.AppendLine($"{GetAttendeeEmote(attendee)} {attendee.DiscordDisplayName}");
            }

            return sb.ToString() ?? string.Empty;
        }

        private string GetAttendeeEmote(AttendeeRecord attendee)
        {
            string role = attendee.Role.ToLower();

            if (role == "absent")
                return "❌";

            if (role == "tentative")
                return "🧇";

            if (role == "attendee")
                return "✅";

            //TODO: Identify custom emotes based on class and spec for WoW events. Non-wow events are handled above.

            string attendeeEmoteCode = _emoteFinder.GetEmote(attendee.Class, attendee.Spec);

            if (!string.IsNullOrWhiteSpace(attendeeEmoteCode))
            {
                return $"<:{attendee.Spec.ToLower()}:{attendeeEmoteCode}>";
            }

            return "<:rocket:1234567890>";
        }

        public string GetRandomFooter()
        {
            string[] possibleFooters =
            [
                "Frenzied Regeneration",
                "EggBoi",
                "Having a Good Time",
                "Zug Zug",
                "Brain Cell",
                "Stay Close to the Nipple",
                "On me! On me!",
                "MY BODY!",
                "Witawy",
                "Pet Pulling...",
            ];

            int footerToReturn = Random.Shared.Next(0, possibleFooters.Length);

            return possibleFooters[footerToReturn];

        }


        public Embed CreateInstanceEmbed(WoWInstanceInfoEntity instanceInfo)
        {
            Color color = instanceInfo.Legacy ? Color.Red : Color.Green;

            string description = instanceInfo.Legacy ? $"Legacy {instanceInfo.PartitionKey}" : instanceInfo.PartitionKey;

            return new EmbedBuilder()
                .WithTitle(instanceInfo.Name)
                .WithDescription(description)
                .WithColor(color)
                .AddField("Database ID", instanceInfo.RowKey)
                .WithThumbnailUrl(instanceInfo.ImageUrl)
                .Build();
        }

        public Embed CreateInstanceEmbed(IEnumerable<WoWInstanceInfoEntity> instanceInfos)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Stored Instances")
                .WithDescription("Here are the instances currently stored in the database.")
                .WithColor(Color.Green);

            var raids = instanceInfos.Where(e => e.PartitionKey == InstanceType.Raid.ToString());
            var dungeons = instanceInfos.Where(e => e.PartitionKey == InstanceType.Dungeon.ToString());

            if (raids.Any())
            {
                embedBuilder.AddField("__Raids__", GetStoredInstanceString(raids));
            }

            if (dungeons.Any())
            {
                embedBuilder.AddField("__Dungeons__", GetStoredInstanceString(dungeons));
            }

            return embedBuilder.Build();

        }

        private string GetStoredInstanceString(IEnumerable<WoWInstanceInfoEntity> instances)
        {
            StringBuilder sb = new();

            foreach (WoWInstanceInfoEntity instance in instances)
            {
                sb.AppendLine($"{instance.Name}");
                sb.AppendLine($"ID: {instance.RowKey}");
            }

            return sb.ToString();
        }

        public async Task<Embed> CreateCharacterEmbedAsync(WoWCharacterEntity character)
        {
            Color color;

            switch (character.Class)
            {
                case "death_knight":
                    color = new Color(196, 30, 58); // Dark Red
                    break;
                case "demon_hunter":
                    color = new Color(163, 48, 201); // Dark Magenta (Fel Green)
                    break;
                case "druid":
                    color = new Color(255, 125, 10); // Orange
                    break;
                case "evoker":
                    color = new Color(51, 147, 127); // Teal
                    break;
                case "hunter":
                    color = new Color(170, 211, 114); // Green
                    break;
                case "mage":
                    color = new Color(105, 204, 240); // Light Blue
                    break;
                case "monk":
                    color = new Color(0, 255, 152); // Jade Green
                    break;
                case "paladin":
                    color = new Color(245, 140, 186); // Pink
                    break;
                case "priest":
                    color = new Color(255, 255, 255); // White
                    break;
                case "rogue":
                    color = new Color(255, 244, 104); // Yellow
                    break;
                case "shaman":
                    color = new Color(0, 112, 222); // Dark Blue
                    break;
                case "warlock":
                    color = new Color(135, 136, 238); // Purple
                    break;
                case "warrior":
                    color = new Color(198, 155, 109); // Brown
                    break;
                default:
                    color = Color.Default; // Default color in Discord.Net
                    break;
            }

            var builder = new EmbedBuilder()
                .WithTitle(character.CharacterName)
                .WithDescription($"{character.Class.Prettyfy()} from the {character.CharacterRealm} realm.")
                .WithColor(color)
                .AddField("Specialization", character.Specialization.Prettyfy());

            builder.ThumbnailUrl = await GetAvatarUrlAsync(character.CharacterName.ToLower(), character.CharacterRealm.ToLower());

            if (!string.IsNullOrWhiteSpace(character.OffSpec))
            {
                builder.AddField("Other Specialization", character.OffSpec);
            }

            return builder.Build();
        }

        private async Task<string> GetAvatarUrlAsync(string characterName, string characterRealm)
        {
            string endpoint = $"profile/wow/character/{characterRealm}/{characterName}/character-media?namespace=profile-us&locale=en_US";

            CharacterMediaResponse response = await _wowApiService.Get<CharacterMediaResponse>(endpoint);

            return response.GetAvatarUrl();
        }
    }
}
