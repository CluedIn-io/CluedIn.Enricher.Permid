using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class AssociatedPerson
    {
        [JsonProperty("Person URL")]
        public List<string> PersonUrl { get; set; }

        [JsonProperty("Honorific prefix", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HonorificPrefix { get; set; }

        [JsonProperty("Given name", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> GivenName { get; set; }

        [JsonProperty("Middle name", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> MiddleName { get; set; }

        [JsonProperty("Family name", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FamilyName { get; set; }

        [JsonProperty("hasReportedTitle.person")]
        public List<string> HasReportedTitlePerson { get; set; }

        [JsonProperty("Position URL")]
        public List<string> PositionUrl { get; set; }

        [JsonProperty("Position Type")]
        public List<string> PositionType { get; set; }

        [JsonProperty("Position Rank")]
        public List<long> PositionRank { get; set; }

        [JsonProperty("Position Start Date")]
        public List<string> PositionStartDate { get; set; }

        [JsonProperty("entityType")]
        public List<string> EntityType { get; set; }

        [JsonProperty("Preferred name", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PreferredName { get; set; }
    }
}