namespace EGON.DiscordBot.Utility
{
    [Serializable]
    public class EnvironmentNotConfiguredException : Exception
    {
        public EnvironmentNotConfiguredException() : base() { }

        public EnvironmentNotConfiguredException(string keyName) : base($"{keyName} is not configured.") { }
    }
}
