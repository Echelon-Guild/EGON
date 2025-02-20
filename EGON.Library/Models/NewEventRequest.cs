using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.Library.Models
{
    public class NewEventRequest
    {
        public Guid InteractionId { get; set; }
        public EventType EventType { get; set; }
        public string OrganizerDisplayName { get; set; }
        public string OrganizerTimeZone { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
    }
}
