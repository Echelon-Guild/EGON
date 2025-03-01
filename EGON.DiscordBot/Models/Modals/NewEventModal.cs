using Discord.Interactions;

namespace EGON.DiscordBot.Models.Modals
{
    public class NewEventModal : IModal
    {
        public string Title => "Create a new event";

        [InputLabel("Name")]
        [ModalTextInput("Name")]
        public string Name { get; set; }

        [InputLabel("Description")]
        [ModalTextInput("Description")]
        public string Description { get; set; }

        [InputLabel("Date/Time of event")]
        [ModalTextInput("dateTimeOfEvent")]
        public DateTime DateTimeOfEvent { get; set; }
    }
}
