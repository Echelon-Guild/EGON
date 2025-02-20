using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.DiscordBot.Models
{
    public class ScheduledMessage
    {
        public ulong UserId { get; set; }
        public string EventId { get; set; }
        public string Message { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
