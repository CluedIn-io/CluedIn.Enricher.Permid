using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class QuoteEntity
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("hasName")]
        public string HasName { get; set; }

        [JsonProperty("assetClass")]
        public string AssetClass { get; set; }

        [JsonProperty("isQuoteOfInstrumentName")]
        public string IsQuoteOfInstrumentName { get; set; }

        [JsonProperty("hasRIC")]
        public string HasRic { get; set; }

        [JsonProperty("hasMic")]
        public string HasMic { get; set; }

        [JsonProperty("hasExchangeTicker")]
        public string HasExchangeTicker { get; set; }

        [JsonProperty("isQuoteOf")]
        public string IsQuoteOf { get; set; }
    }
}