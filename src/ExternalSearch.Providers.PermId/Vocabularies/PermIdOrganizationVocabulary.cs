// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdOrganizationVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier organization vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.PermId.Vocabularies
{
    public class PermIdOrganizationVocabulary : PermIdVocabulary
    {
        public PermIdOrganizationVocabulary()
        {
            this.VocabularyName = "PermId Organization";
            this.KeyPrefix = "permId.organization";
            this.KeySeparator = ".";
            this.Grouping = CluedIn.Core.Data.EntityType.Organization;

            this.AddGroup("PermId Organization Details", group =>
            {
                this.DomiciledIn = group.Add(new VocabularyKey("domiciledIn", VocabularyKeyDataType.GeographyCountry));
                this.Status = group.Add(new VocabularyKey("status"));
                this.Public = group.Add(new VocabularyKey("public", VocabularyKeyDataType.Boolean));
                this.OrganizationName = group.Add(new VocabularyKey("organizationName", VocabularyKeyDataType.OrganizationName));
                this.Lei = group.Add(new VocabularyKey("lei"));
                this.LatestDateOfIncorporation = group.Add(new VocabularyKey("latestDateOfIncorporation", VocabularyKeyDataType.DateTime));
                this.IncorporatedIn = group.Add(new VocabularyKey("incorporatedIn", VocabularyKeyDataType.GeographyCountry));
                this.Website = group.Add(new VocabularyKey("website", VocabularyKeyDataType.Uri));
                this.EntityType = group.Add(new VocabularyKey("entityType"));
            });

            this.AddGroup("Contact Information", group =>
            {
                this.HqAddress = group.Add(new VocabularyKey("hqAddress", VocabularyKeyDataType.GeographyLocation));
                this.HqPhone = group.Add(new VocabularyKey("hqPhone", VocabularyKeyDataType.PhoneNumber));
                this.HqFax = group.Add(new VocabularyKey("hqFax", VocabularyKeyDataType.PhoneNumber));
                this.RegisteredPhone = group.Add(new VocabularyKey("registeredPhone", VocabularyKeyDataType.PhoneNumber));
                this.RegisteredFax = group.Add(new VocabularyKey("registeredFax", VocabularyKeyDataType.PhoneNumber));
                this.RegisteredAddress = group.Add(new VocabularyKey("registeredAddress", VocabularyKeyDataType.GeographyLocation));
            });

            this.AddGroup("Business Information", group =>
            {
                this.PrimaryIndustryId = group.Add(new VocabularyKey("primaryIndustryId", VocabularyKeyVisibility.Hidden));
                this.PrimaryIndustry = group.Add(new VocabularyKey("primaryIndustry"));
                this.PrimaryEconomicSectorId = group.Add(new VocabularyKey("primaryEconomicSectorId", VocabularyKeyVisibility.Hidden));
                this.PrimaryEconomicSector = group.Add(new VocabularyKey("primaryEconomicSector"));
                this.PrimaryBusinessSectorId = group.Add(new VocabularyKey("primaryBusinessSectorId", VocabularyKeyVisibility.Hidden));
                this.PrimaryBusinessSector = group.Add(new VocabularyKey("primaryBusinessSector"));
            });

            this.AddGroup("Primary Instrument", group =>
            {
                this.PrimaryInstrumentId = group.Add(new VocabularyKey("primaryInstrument.id", VocabularyKeyDataType.Number, VocabularyKeyVisibility.Hidden));
                this.PrimaryInstrumentName = group.Add(new VocabularyKey("primaryInstrument.name"));
                this.PrimaryInstrumentType = group.Add(new VocabularyKey("primaryInstrument.type"));
                this.PrimaryInstrumentTypeUrl = group.Add(new VocabularyKey("primaryInstrument.typeUrl", VocabularyKeyDataType.Uri));
                this.PrimaryInstrumentUrl = group.Add(new VocabularyKey("primaryInstrument.url", VocabularyKeyDataType.Uri));
            });

            this.AddGroup("Main Quote", group =>
                {
                    this.MainQuoteId = group.Add(new VocabularyKey("mainQuote.id", VocabularyKeyDataType.Number, VocabularyKeyVisibility.Hidden));
                    this.MainQuoteExchange = group.Add(new VocabularyKey("mainQuote.exchange"));
                    this.MainQuoteMic = group.Add(new VocabularyKey("mainQuote.mic"));
                    this.MainQuoteRic = group.Add(new VocabularyKey("mainQuote.ric"));
                    this.MainQuoteTicker = group.Add(new VocabularyKey("mainQuote.ticker"));
                    this.MainQuoteUrl = group.Add(new VocabularyKey("mainQuote.url", VocabularyKeyDataType.Uri));
                });

            this.AddMapping(this.Website, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website);

        }

        public VocabularyKey DomiciledIn { get; set; }
        public VocabularyKey HqAddress { get; set; }
        public VocabularyKey Status { get; set; }
        public VocabularyKey RegisteredPhone { get; set; }
        public VocabularyKey RegisteredFax { get; set; }
        public VocabularyKey RegisteredAddress { get; set; }
        public VocabularyKey Public { get; set; }
        public VocabularyKey PrimaryIndustryId { get; set; }
        public VocabularyKey PrimaryIndustry { get; set; }
        public VocabularyKey PrimaryEconomicSectorId { get; set; }
        public VocabularyKey PrimaryEconomicSector { get; set; }
        public VocabularyKey PrimaryBusinessSectorId { get; set; }
        public VocabularyKey PrimaryBusinessSector { get; set; }
        public VocabularyKey OrganizationName { get; set; }
        public VocabularyKey Lei { get; set; }
        public VocabularyKey LatestDateOfIncorporation { get; set; }
        public VocabularyKey IncorporatedIn { get; set; }
        public VocabularyKey HqPhone { get; set; }
        public VocabularyKey HqFax { get; set; }
        public VocabularyKey Website { get; set; }
        public VocabularyKey EntityType { get; set; }

        public VocabularyKey PrimaryInstrumentId { get; set; }
        public VocabularyKey PrimaryInstrumentName { get; set; }
        public VocabularyKey PrimaryInstrumentType { get; set; }
        public VocabularyKey PrimaryInstrumentTypeUrl { get; set; }
        public VocabularyKey PrimaryInstrumentUrl { get; set; }

        public VocabularyKey MainQuoteId { get; set; }
        public VocabularyKey MainQuoteUrl { get; set; }
        public VocabularyKey MainQuoteExchange { get; set; }
        public VocabularyKey MainQuoteMic { get; set; }
        public VocabularyKey MainQuoteRic { get; set; }
        public VocabularyKey MainQuoteTicker { get; set; }
    }
}
