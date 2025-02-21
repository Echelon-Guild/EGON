namespace EGON.DiscordBot.Models
{
    public class WoWCharacter
    {
        public Guid Id { get; set; }
        public string CharacterName { get; set; }
        public string CharacterRealm { get; set; }
        public string RegisteredTo { get; set; }
        public string Class { get; set; }
        public string Specialization { get; set; }
        public string OffSpec { get; set; }
    }
}
