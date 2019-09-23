using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class HasPrimaryIndustryGroup
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
    }
}