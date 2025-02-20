﻿using Azure.Data.Tables;
using EGON.Library.Models.Entities;
using EGON.Library.Utility;

namespace EGON.Library.Services
{
    public class EmoteFinder
    {
        private TableClient _storedEmotes;

        public EmoteFinder(TableServiceClient tableServiceClient)
        {
            _storedEmotes = tableServiceClient.GetTableClient(TableNames.EMOTE_TABLE_NAME);
            _storedEmotes.CreateIfNotExists();
        }

        public string GetEmote(string className, string spec)
        {
            StoredEmoteEntity entity = _storedEmotes.Query<StoredEmoteEntity>(e => e.PartitionKey == className.ToLower() && e.RowKey == spec.ToLower()).First();

            return entity.EmoteID ?? "❌";
        }
    }
}
