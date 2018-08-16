using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayServices
{
    class GameCompatibility
    {
        public string GameId { get; set; }
        public string State { get; set; }
    }

    public class CompatibilitySummaryItem
    {
        [JsonProperty("state")]
        public string State { get; set; } = String.Empty;

        [JsonProperty("count")]
        public int Count { get; set; }
    };

    public class CompatibilitySummary
    {
        [JsonProperty("items")]
        public List<CompatibilitySummaryItem> Items { get; set; } = new List<CompatibilitySummaryItem>();

        [JsonProperty("updateTime")]
        public DateTime UpdateTime { get; set; }
    };
}
