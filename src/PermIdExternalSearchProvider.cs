// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdExternalSearchProvider.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier external search provider class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Newtonsoft.Json;
using RestSharp;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Relational;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.PermId.Models;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Providers.PermId.Vocabularies;
using EntityType = CluedIn.Core.Data.EntityType;

namespace CluedIn.ExternalSearch.Providers.PermId
{
    /// <summary>The permid graph external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class PermIdExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/
        
        public PermIdExternalSearchProvider()
            : this(true)
        {
            var nameBasedTokenProvider = new NameBasedTokenProvider("PermId");

            if (nameBasedTokenProvider.ApiToken != null)
                this.TokenProvider = new RoundRobinTokenProvider(nameBasedTokenProvider.ApiToken.Split(',', ';'));
        }

        public PermIdExternalSearchProvider(IList<string> tokens)
            : this(true)
        {
            this.TokenProvider = new RoundRobinTokenProvider(tokens);
        }
 
        public PermIdExternalSearchProvider([NotNull] IExternalSearchTokenProvider tokenProvider)
            : this(true)
        {
            this.TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        private PermIdExternalSearchProvider(bool tokenProviderIsRequired)
            : base(Constants.ExternalSearchProviders.PermId, EntityType.Organization)
        {
            this.TokenProviderIsRequired = tokenProviderIsRequired;
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <inheritdoc/>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            if (string.IsNullOrEmpty(this.TokenProvider.ApiToken))
                throw new InvalidOperationException("PermId ApiToken have not been configured");
 
            var existingResults = request.GetQueryResults<PermIdSocialResponse>(this).ToList();

            Func<string, bool> existingDataFilter   = value => existingResults.Any(r => string.Equals(r.Data.OrganizationName.First(), value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> nameFilter           = value => OrganizationFilters.NameFilter(context, value) || existingResults.Any(r => string.Equals(r.Data.OrganizationName.First(), value, StringComparison.InvariantCultureIgnoreCase));

            // Query Input
            var entityType       = request.EntityMetaData.EntityType;
            var organizationName = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, new HashSet<string>());

            if (!string.IsNullOrEmpty(request.EntityMetaData.Name))
                organizationName.Add(request.EntityMetaData.Name);
            if (!string.IsNullOrEmpty(request.EntityMetaData.DisplayName))
                organizationName.Add(request.EntityMetaData.DisplayName);

            if (organizationName != null)
            {
                var values = organizationName.GetOrganizationNameVariants()
                                             .Select(NameNormalization.Normalize)
                                             .ToHashSet();

                foreach (var value in values)
                {
                    if (existingDataFilter(value) || nameFilter(value))
                        continue;

                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Name, value);
                }
            }
        }
 
        /// <inheritdoc/>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var name     = query.QueryParameters[ExternalSearchQueryParameter.Name].FirstOrDefault();
            var idList   = new List<string>();
            var apiToken = this.TokenProvider.ApiToken;

            if (string.IsNullOrEmpty(apiToken))
                throw new InvalidOperationException("PermId ApiToken have not been configured");

            if (string.IsNullOrEmpty(name))
                yield break;

            var searchClient = new RestClient("https://api.thomsonreuters.com/permid/");
            var socialClient = new RestClient("https://permid.org/api/mdaas/getEntityById/");

            if (!string.IsNullOrEmpty(name))
            {
                var searchResult = this.RequestWrapper<PermIdSearchResponse>(searchClient, "search?q=" + name);

                foreach (var res in searchResult.Result.Organizations.Entities)
                {
                    idList.Add(res.Id.Split('-')[1]);
                }
            }

            foreach (var permId in idList)
            {
                var socialResult = this.RequestWrapper<PermIdSocialResponse>(socialClient, permId);

                if (socialResult != null)
                {
                    yield return new ExternalSearchQueryResult<PermIdSocialResponse>(query, socialResult);
                }
            }
        }

        private T RequestWrapper<T>(IRestClient client, string parameter)
        {
            T retval = default(T);

            ActionExtensions.ExecuteWithRetry(() =>
            {
                var request = new RestRequest(parameter, Method.GET);

                request.AddHeader("X-AG-Access-Token", this.TokenProvider.ApiToken);

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    retval = JsonConvert.DeserializeObject<T>(response.Content);
                }
                else
                {
                    throw new WebException("Http status code: " + response.StatusCode.ToString());
                }
            });

            return retval;
        }
 
        /// <inheritdoc/>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var organizationCode = this.GetOriginEntityCode(result.As<PermIdSocialResponse>());
            var organizationClue = new Clue(organizationCode, context.Organization);

            this.PopulateMetadata(organizationClue.Data.EntityData, result.As<PermIdSocialResponse>());

            yield return organizationClue;

            if (result.As<PermIdSocialResponse>().Data.AdditionalInfo != null)
            {
                foreach (var person in result.As<PermIdSocialResponse>().Data.AdditionalInfo)
                {
                    var personCode = this.GetPersonEntityCode(person);
                    var personClue = new Clue(personCode, context.Organization);

                    this.PopulatePersonMetadata(personClue.Data.EntityData, person);

                    var personToOrganizationEdge = new EntityEdge(new EntityReference(personCode), new EntityReference(organizationCode), EntityEdgeType.WorksFor);
                    personClue.Data.EntityData.OutgoingEdges.Add(personToOrganizationEdge);

                    yield return personClue;
                }
            }
        }
 
        /// <inheritdoc/>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<PermIdSocialResponse>();

            if (resultItem == null)
                return null;

            return this.CreateMetadata(resultItem);
        }

        /// <inheritdoc/>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<PermIdSocialResponse> resultItem)
        {
            if (resultItem == null)
                throw new ArgumentNullException(nameof(resultItem));

            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);
 
            return metadata;
        }
 
        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<PermIdSocialResponse> resultItem)
        {
            if (resultItem == null)
                throw new ArgumentNullException(nameof(resultItem));

            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.PermId.First());
        }

        /// <summary>Gets person entity code.</summary>
        /// <param name="person">The person.</param>
        /// <returns>The person entity code.</returns>
        private EntityCode GetPersonEntityCode(AssociatedPerson person)
        {
            return new EntityCode(EntityType.Infrastructure.User, this.GetCodeOrigin(), person.PersonUrl.First());
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("permid");
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<PermIdSocialResponse> resultItem)
        {
            if (resultItem == null)
                throw new ArgumentNullException(nameof(resultItem));

            var code = this.GetOriginEntityCode(resultItem);
            var data = resultItem.Data;
            
            metadata.EntityType  = EntityType.Organization;
            metadata.Name        = resultItem.Data.OrganizationName?.FirstOrDefault().PrintIfAvailable();
            metadata.CreatedDate = resultItem.CreatedDate;

            metadata.OriginEntityCode = code;
            metadata.Codes.Add(code);

            metadata.Properties[PermIdVocabularies.Organization.PermId]                     = data.PermId?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.DomiciledIn]                = data.DomiciledIn?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.HqAddress]                  = data.HqAddress?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.Status]                     = data.Status?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.RegisteredPhone]            = data.RegisteredPhone?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.RegisteredFax]              = data.RegisteredFax?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.RegisteredAddress]          = data.RegisteredAddress?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.Public]                     = data.Public?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryIndustryId]          = data.PrimaryIndustryId?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryIndustry]            = data.PrimaryIndustry?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryEconomicSectorId]    = data.PrimaryEconomicSectorId?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryEconomicSector]      = data.PrimaryEconomicSector?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryBusinessSectorId]    = data.PrimaryBusinessSectorId?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryBusinessSector]      = data.PrimaryBusinessSector?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.OrganizationName]           = data.OrganizationName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.Lei]                        = data.Lei?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.LatestDateOfIncorporation]  = data.LatestDateOfIncorporation?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.IncorporatedIn]             = data.IncorporatedIn?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.HqPhone]                    = data.HqPhone?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.HqFax]                      = data.HqFax?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.EntityType]                 = data.EntityType?.FirstOrDefault().PrintIfAvailable();

            metadata.Properties[PermIdVocabularies.Organization.PrimaryInstrumentId]        = data.PrimaryInstrumentMdaas?.FirstOrDefault()?.MainInstrument?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryInstrumentName]      = data.PrimaryInstrumentMdaas?.FirstOrDefault()?.MainInstrumentName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryInstrumentType]      = data.PrimaryInstrumentMdaas?.FirstOrDefault()?.MainInstrumentType?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryInstrumentTypeUrl]   = data.PrimaryInstrumentMdaas?.FirstOrDefault()?.MainInstrumentTypeUrl?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.PrimaryInstrumentUrl]       = data.PrimaryInstrumentMdaas?.FirstOrDefault()?.MainInstrumentUrl?.FirstOrDefault().PrintIfAvailable();

            metadata.Properties[PermIdVocabularies.Organization.MainQuoteId]                = data.MainQuoteIdMdaas?.FirstOrDefault()?.MainQuote?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.MainQuoteUrl]               = data.MainQuoteIdMdaas?.FirstOrDefault()?.MainQuoteUrl?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.MainQuoteExchange]          = data.MainQuoteIdMdaas?.FirstOrDefault()?.PrimaryExchange?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.MainQuoteMic]               = data.MainQuoteIdMdaas?.FirstOrDefault()?.PrimaryMic?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.MainQuoteRic]               = data.MainQuoteIdMdaas?.FirstOrDefault()?.PrimaryRic?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Organization.MainQuoteTicker]            = data.MainQuoteIdMdaas?.FirstOrDefault()?.PrimaryTicker?.FirstOrDefault().PrintIfAvailable();

            // Set Url
            if (!string.IsNullOrEmpty(resultItem.Data.Website?.FirstOrDefault()))
            {
                metadata.Uri = new Uri(resultItem.Data.Website.First());
                metadata.Properties[PermIdVocabularies.Organization.Website] = resultItem.Data.Website.First();
            }
            else if (!string.IsNullOrEmpty(resultItem.Data.PermId?.FirstOrDefault()))
            {
                metadata.Uri = new Uri(string.Format("https://permid.org/1-{0}", resultItem.Data.PermId.First()));
            }
        }

        /// <summary>Populate person metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="person">The person.</param>
        private void PopulatePersonMetadata(IEntityMetadata metadata, AssociatedPerson person)
        {
            var code = this.GetPersonEntityCode(person);

            metadata.EntityType = EntityType.Infrastructure.User;
            var name = string.Empty;

            if (!string.IsNullOrEmpty(person.GivenName?.FirstOrDefault())) name += person.GivenName.First();
            if (!string.IsNullOrEmpty(person.MiddleName?.FirstOrDefault())) name += " " + person.MiddleName.First();
            if (!string.IsNullOrEmpty(person.FamilyName?.FirstOrDefault())) name += " " + person.FamilyName.First();

            metadata.Name             = name;
            metadata.OriginEntityCode = code;
            metadata.Codes.Add(code);

            metadata.Properties[PermIdVocabularies.Person.PersonUrl]                = person.PersonUrl?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.HonorificPrefix]          = person.HonorificPrefix?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.GivenName]                = person.GivenName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.MiddleName]               = person.MiddleName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.FamilyName]               = person.FamilyName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.HasReportedTitlePerson]   = person.HasReportedTitlePerson?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.PositionUrl]              = person.PositionUrl?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.PositionType]             = person.PositionType?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.PositionRank]             = person.PositionRank?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.PositionStartDate]        = person.PositionStartDate?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.PreferredName]            = person.PreferredName?.FirstOrDefault().PrintIfAvailable();
            metadata.Properties[PermIdVocabularies.Person.EntityType]               = person.EntityType?.FirstOrDefault().PrintIfAvailable();

            // Set Url
            if (!string.IsNullOrEmpty(person.PersonUrl?.FirstOrDefault()))
                metadata.Uri = new Uri(string.Format("https://permid.org/1-{0}", person.PersonUrl.First()));
        }

        public string Icon { get; } = "Resources.permid.jpg";
        public string Domain { get; } = "https://permid.org/";
        public string About { get; } = "PermID is an enricher using permanent and universal identifiers where underlying attributes capture the context of the identity they each represent";
        public AuthMethods AuthMethods { get; } = null;
        public IEnumerable<Control> Properties { get; } = null;
        public Guide Guide { get; } = null;
        public IntegrationType Type { get; } = IntegrationType.Cloud;
    }
}
