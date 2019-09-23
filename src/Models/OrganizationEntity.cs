using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class OrganizationEntity
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("organizationName")]
        public string OrganizationName { get; set; }

        [JsonProperty("primaryTicker")]
        public string PrimaryTicker { get; set; }

        [JsonProperty("orgSubtype")]
        public string OrgSubtype { get; set; }

        [JsonProperty("hasHoldingClassification")]
        public string HasHoldingClassification { get; set; }
        
        [JsonProperty("hasURL")]
        public string HasUrl { get; set; }
    }
}