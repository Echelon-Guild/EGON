using Discord.Interactions;

namespace EGON.DiscordBot.Models.Modals
{
    public class ChangeEventDateModal : IModal
    {
        public string Title => "Change the date of an event";

        [InputLabel("Date/Time of event")]
        [ModalTextInput("dateTimeOfEvent")]
        public DateTime DateTimeOfEvent { get; set; }
    }
}
