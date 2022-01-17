using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public partial class MainQuoteIdMdaa
    {
        [JsonProperty("Main Quote")]
        public List<string> MainQuote { get; set; }

        [JsonProperty("Main Quote URL")]
        public List<string> MainQuoteUrl { get; set; }

        [JsonProperty("Primary RIC")]
        public List<string> PrimaryRic { get; set; }

        [JsonProperty("Primary Mic")]
        public List<string> PrimaryMic { get; set; }

        [JsonProperty("Primary Ticker")]
        public List<string> PrimaryTicker { get; set; }

        [JsonProperty("Primary Exchange")]
        public List<string> PrimaryExchange { get; set; }

        [JsonProperty("URL")]
        public List<string> Url { get; set; }
    }
}