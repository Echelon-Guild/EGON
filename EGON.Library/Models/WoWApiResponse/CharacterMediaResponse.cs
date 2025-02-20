using Newtonsoft.Json;

namespace EGON.Library.Models.WoWApiResponse
{
    public class CharacterMediaResponse
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("character")]
        public CharacterInfo Character { get; set; }

        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }

        public string GetAvatarUrl()
        {
            return Assets?.Find(a => a.Key == "avatar")?.Value;
        }
    }

    public class Links
    {
        [JsonProperty("self")]
        public SelfLink Self { get; set; }
    }

    public class SelfLink
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public class CharacterInfo
    {
        [JsonProperty("key")]
        public SelfLink Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("realm")]
        public Realm Realm { get; set; }
    }

    public class Realm
    {
        [JsonProperty("key")]
        public SelfLink Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class Asset
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}

