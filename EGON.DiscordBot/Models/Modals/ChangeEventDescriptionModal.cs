using Discord;
using Discord.Interactions;

namespace EGON.DiscordBot.Models.Modals
{
    public class ChangeEventDescriptionModal : IModal
    {
        public string Title => "Change an event description";

        [InputLabel("Description")]
        [ModalTextInput("Description", TextInputStyle.Paragraph)]
        public string Description { get; set; }
    }
}
