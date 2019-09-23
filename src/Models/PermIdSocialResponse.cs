// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdSocialResponse.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier social response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public partial class PermIdSocialResponse
    {
        [JsonProperty("Domiciled in")]
        public List<string> DomiciledIn { get; set; }

        [JsonProperty("Incorporated in")]
        public List<string> IncorporatedIn { get; set; }

        [JsonProperty("Public")]
        public List<string> Public { get; set; }

        [JsonProperty("Latest Date of Incorporation")]
        public List<DateTimeOffset> LatestDateOfIncorporation { get; set; }

        [JsonProperty("Organization Name")]
        public List<string> OrganizationName { get; set; }

        [JsonProperty("PERM ID")]
        public List<string> PermId { get; set; }

        [JsonProperty("Status")]
        public List<string> Status { get; set; }

        [JsonProperty("Website")]
        public List<string> Website { get; set; }

        [JsonProperty("HQ Address")]
        public List<string> HqAddress { get; set; }

        [JsonProperty("Registered Address")]
        public List<string> RegisteredAddress { get; set; }

        [JsonProperty("HQ Phone")]
        public List<string> HqPhone { get; set; }

        [JsonProperty("HQ Fax")]
        public List<string> HqFax { get; set; }

        [JsonProperty("Registered Phone")]
        public List<string> RegisteredPhone { get; set; }

        [JsonProperty("Registered Fax")]
        public List<string> RegisteredFax { get; set; }

        [JsonProperty("Primary industry")]
        public List<string> PrimaryIndustry { get; set; }

        [JsonProperty("Primary Industry ID")]
        public List<string> PrimaryIndustryId { get; set; }

        [JsonProperty("Primary Business Sector")]
        public List<string> PrimaryBusinessSector { get; set; }

        [JsonProperty("Primary Business Sector ID")]
        public List<string> PrimaryBusinessSectorId { get; set; }

        [JsonProperty("Primary Economic Sector")]
        public List<string> PrimaryEconomicSector { get; set; }

        [JsonProperty("Primary Economic Sector ID")]
        public List<string> PrimaryEconomicSectorId { get; set; }

        [JsonProperty("primaryInstrument.mdaas")]
        public List<PrimaryInstrumentMdaa> PrimaryInstrumentMdaas { get; set; }

        [JsonProperty("LEI")]
        public List<string> Lei { get; set; }

        [JsonProperty("mainQuoteId.mdaas")]
        public List<MainQuoteIdMdaa> MainQuoteIdMdaas { get; set; }

        [JsonProperty("entityType")]
        public List<string> EntityType { get; set; }

        [JsonProperty("Additional Info")]
        public List<AssociatedPerson> AdditionalInfo { get; set; }
    }
}