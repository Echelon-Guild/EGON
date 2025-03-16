using Discord;
using Discord.Interactions;

namespace EGON.DiscordBot.Models.Modals
{
    public class EditEventModal : IModal
    {
        public string Title { get; set; }

        [InputLabel("Name")]
        [ModalTextInput("Name")]
        [RequiredInput(false)]
        public string Name { get; set; }

        [InputLabel("Description")]
        [ModalTextInput("Description", TextInputStyle.Paragraph)]
        [RequiredInput(false)]
        public string Description { get; set; }

        [InputLabel("Date/Time of event")]
        [ModalTextInput("dateTimeOfEvent")]
        [RequiredInput(false)]
        public string DateTimeOfEvent { get; set; }
    }
}
