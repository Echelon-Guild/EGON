using System.Text.Json.Serialization;

namespace EGON.Library.Models.WoWApiResponse
{
    public class CharacterMediaResponse
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }

        [JsonPropertyName("character")]
        public CharacterInfo Character { get; set; }

        [JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; }

        public string GetAvatarUrl()
        {
            return Assets?.Find(a => a.Key == "avatar")?.Value;
        }
    }

    public class Links
    {
        [JsonPropertyName("self")]
        public SelfLink Self { get; set; }
    }

    public class SelfLink
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class CharacterInfo
    {
        [JsonPropertyName("key")]
        public SelfLink Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("realm")]
        public Realm Realm { get; set; }
    }

    public class Realm
    {
        [JsonPropertyName("key")]
        public SelfLink Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}

