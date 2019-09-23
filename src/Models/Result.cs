using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class Result
    {
        [JsonProperty("organizations")]
        public Organization Organizations { get; set; }

        [JsonProperty("instruments")]
        public Instrument Instruments { get; set; }

        [JsonProperty("quotes")]
        public Quote Quotes { get; set; }
    }
}