using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class Context
    {
        [JsonProperty("organization-name")]
        public HasPrimaryIndustryGroup OrganizationName { get; set; }

        [JsonProperty("hasURL")]
        public HasPrimaryIndustryGroup HasUrl { get; set; }

        [JsonProperty("HeadquartersAddress")]
        public HasPrimaryIndustryGroup HeadquartersAddress { get; set; }

        [JsonProperty("hasRegisteredPhoneNumber")]
        public HasPrimaryIndustryGroup HasRegisteredPhoneNumber { get; set; }

        [JsonProperty("hasHeadquartersPhoneNumber")]
        public HasPrimaryIndustryGroup HasHeadquartersPhoneNumber { get; set; }

        [JsonProperty("hasPrimaryIndustryGroup")]
        public HasPrimaryIndustryGroup HasPrimaryIndustryGroup { get; set; }

        [JsonProperty("hasActivityStatus")]
        public HasPrimaryIndustryGroup HasActivityStatus { get; set; }

        [JsonProperty("isIncorporatedIn")]
        public HasPrimaryIndustryGroup IsIncorporatedIn { get; set; }

        [JsonProperty("hasRegisteredFaxNumber")]
        public HasPrimaryIndustryGroup HasRegisteredFaxNumber { get; set; }

        [JsonProperty("hasLEI")]
        public HasPrimaryIndustryGroup HasLei { get; set; }

        [JsonProperty("hasPrimaryBusinessSector")]
        public HasPrimaryIndustryGroup HasPrimaryBusinessSector { get; set; }

        [JsonProperty("hasPermId")]
        public HasPrimaryIndustryGroup HasPermId { get; set; }

        [JsonProperty("hasHeadquartersFaxNumber")]
        public HasPrimaryIndustryGroup HasHeadquartersFaxNumber { get; set; }

        [JsonProperty("RegisteredAddress")]
        public HasPrimaryIndustryGroup RegisteredAddress { get; set; }

        [JsonProperty("isDomiciledIn")]
        public HasPrimaryIndustryGroup IsDomiciledIn { get; set; }

        [JsonProperty("hasLatestOrganizationFoundedDate")]
        public HasPrimaryIndustryGroup HasLatestOrganizationFoundedDate { get; set; }

        [JsonProperty("hasPrimaryEconomicSector")]
        public HasPrimaryIndustryGroup HasPrimaryEconomicSector { get; set; }

        [JsonProperty("mdaas")]
        public string Mdaas { get; set; }

        [JsonProperty("tr-fin")]
        public string TrFin { get; set; }

        [JsonProperty("tr-common")]
        public string TrCommon { get; set; }

        [JsonProperty("fibo-be-le-cb")]
        public string FiboBeLeCb { get; set; }

        [JsonProperty("xsd")]
        public string Xsd { get; set; }

        [JsonProperty("vcard")]
        public string Vcard { get; set; }

        [JsonProperty("tr-org")]
        public string TrOrg { get; set; }
    }
}