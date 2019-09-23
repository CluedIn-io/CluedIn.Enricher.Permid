// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdLookupResponse.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier lookup response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
	public class PermIdLookupResponse
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("mdaas:HeadquartersAddress")]
        public string MdaasHeadquartersAddress { get; set; }

        [JsonProperty("mdaas:RegisteredAddress")]
        public string MdaasRegisteredAddress { get; set; }

        [JsonProperty("tr-common:hasPermId")]
        public string TrCommonHasPermId { get; set; }

        [JsonProperty("hasActivityStatus")]
        public string HasActivityStatus { get; set; }

        [JsonProperty("tr-org:hasHeadquartersFaxNumber")]
        public string TrOrgHasHeadquartersFaxNumber { get; set; }

        [JsonProperty("tr-org:hasHeadquartersPhoneNumber")]
        public string TrOrgHasHeadquartersPhoneNumber { get; set; }

        [JsonProperty("tr-org:hasLEI")]
        public string TrOrgHasLei { get; set; }

        [JsonProperty("hasLatestOrganizationFoundedDate")]
        public string HasLatestOrganizationFoundedDate { get; set; }

        [JsonProperty("hasPrimaryBusinessSector")]
        public string HasPrimaryBusinessSector { get; set; }

        [JsonProperty("hasPrimaryEconomicSector")]
        public string HasPrimaryEconomicSector { get; set; }

        [JsonProperty("hasPrimaryIndustryGroup")]
        public string HasPrimaryIndustryGroup { get; set; }

        [JsonProperty("tr-org:hasRegisteredFaxNumber")]
        public string TrOrgHasRegisteredFaxNumber { get; set; }

        [JsonProperty("tr-org:hasRegisteredPhoneNumber")]
        public string TrOrgHasRegisteredPhoneNumber { get; set; }

        [JsonProperty("isIncorporatedIn")]
        public string IsIncorporatedIn { get; set; }

        [JsonProperty("isDomiciledIn")]
        public string IsDomiciledIn { get; set; }

        [JsonProperty("hasURL")]
        public string HasUrl { get; set; }

        [JsonProperty("vcard:organization-name")]
        public string VcardOrganizationName { get; set; }

        [JsonProperty("@context")]
        public Context Context { get; set; }
    }
}
