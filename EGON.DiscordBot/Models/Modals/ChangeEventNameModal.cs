using Discord.Interactions;

namespace EGON.DiscordBot.Models.Modals
{
    public class ChangeEventNameModal : IModal
    {
        public string Title => "Change the name of an event";

        [InputLabel("Name")]
        [ModalTextInput("Name")]
        public string Name { get; set; }
    }
}
