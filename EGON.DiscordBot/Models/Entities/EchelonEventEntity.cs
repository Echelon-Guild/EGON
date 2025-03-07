using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class EchelonEventEntity : ITableEntity
    {
        public string PartitionKey { get; set; }  // E.g., "Raid", "Mythic", "Meeting"
        public string RowKey { get; set; }  // Unique event ID
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public string Organizer { get; set; }
        public string OrganizerUserId { get; set; }
        public string ImageUrl { get; set; }
        public string Footer { get; set; }
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public string MessageUrl { get; set; }

        // This HAS to be a string for the query, a ulong fails to convert properly
        public string EventId { get; set; }

        public bool Closed { get; set; }


        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public EchelonEventEntity() { }

        public EchelonEventEntity(EchelonEvent dto)
        {
            PartitionKey = dto.EventType.ToString();
            RowKey = dto.EventId;
            EventName = dto.Name;
            EventDescription = dto.Description;
            EventDateTime = dto.EventDateTime;
            Organizer = dto.Organizer;
            OrganizerUserId = dto.OrganizerUserId;
            ImageUrl = dto.ImageUrl;
            Footer = dto.Footer;
            MessageId = dto.MessageId;
            ChannelId = dto.ChannelId;
            MessageUrl = dto.MessageUrl;
            EventId = dto.EventId;
            Closed = dto.Closed;
        }

        public EchelonEvent ToDto()
        {
            var ecEvent = new EchelonEvent()
            {
                Name = EventName,
                Description = EventDescription,
                EventDateTime = EventDateTime,
                Organizer = Organizer,
                OrganizerUserId = OrganizerUserId,
                ImageUrl = ImageUrl,
                Footer = Footer,
                MessageId = MessageId,
                MessageUrl = MessageUrl,
                Id = ulong.Parse(EventId),
                EventType = Enum.Parse<EventType>(PartitionKey),
                Closed = Closed,
                ChannelId = ChannelId
            };

            return ecEvent;
        }
    }



}
