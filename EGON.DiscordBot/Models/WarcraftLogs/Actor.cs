using System.Text.Json.Serialization;

namespace EGON.DiscordBot.Models.WarcraftLogs
{
    public class Actor
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("subType")]
        public string? SubType { get; set; }

        [JsonPropertyName("server")]
        public string? Server { get; set; }
    }
}
