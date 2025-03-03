using Azure;
using Azure.Data.Tables;

namespace EGON.DiscordBot.Models.Entities
{
    public class ApprovedCallerEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string DiscordUserName { get; set; }
        public string AuthorizedToCallCommandName { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ApprovedCallerEntity() { }

        public ApprovedCallerEntity(ApprovedCaller dto)
        {
            PartitionKey = "ApprovedCallers";
            RowKey = dto.DiscordUserName;

            DiscordUserName = dto.DiscordUserName;
            AuthorizedToCallCommandName = dto.AuthorizedToCallCommandName;
        }

        public ApprovedCaller ToDto()
        {
            var approvedCaller = new ApprovedCaller()
            {
                DiscordUserName = DiscordUserName,
                AuthorizedToCallCommandName = AuthorizedToCallCommandName
            };

            return approvedCaller;
        }
    }
}
