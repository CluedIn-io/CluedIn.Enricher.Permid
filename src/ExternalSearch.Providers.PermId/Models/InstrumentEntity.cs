using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class InstrumentEntity
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("hasName")]
        public string HasName { get; set; }

        [JsonProperty("assetClass")]
        public string AssetClass { get; set; }

        [JsonProperty("isIssuedByName")]
        public string IsIssuedByName { get; set; }

        [JsonProperty("isIssuedBy")]
        public string IsIssuedBy { get; set; }

        [JsonProperty("hasPrimaryQuote")]
        public string HasPrimaryQuote { get; set; }

        [JsonProperty("primaryTicker")]
        public string PrimaryTicker { get; set; }
    }
}