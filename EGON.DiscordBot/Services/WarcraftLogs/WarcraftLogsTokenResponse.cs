﻿using System.Text.Json.Serialization;

namespace EGON.DiscordBot.Services.WarcraftLogs
{
    public class WarcraftLogsTokenResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
