using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public partial class PrimaryInstrumentMdaa
    {
        [JsonProperty("Main Instrument")]
        public List<string> MainInstrument { get; set; }

        [JsonProperty("Main Instrument URL")]
        public List<string> MainInstrumentUrl { get; set; }

        [JsonProperty("Main Instrument Name")]
        public List<string> MainInstrumentName { get; set; }

        [JsonProperty("Main instrument type")]
        public List<string> MainInstrumentType { get; set; }

        [JsonProperty("Main instrument type URL")]
        public List<string> MainInstrumentTypeUrl { get; set; }

        [JsonProperty("URL")]
        public List<string> Url { get; set; }
    }
}