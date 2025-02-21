using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.Library.Models
{
    public class ScheduledMessage
    {
        public ulong UserId { get; set; }
        public ulong EventId { get; set; }
        public string Message { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
