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
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.PermId
{
    /// <summary>The permid graph external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class PermIdExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata , IConfigurableExternalSearchProvider
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private static readonly EntityType[] DefaultAcceptedEntityTypes = { EntityType.Organization };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public PermIdExternalSearchProvider()
            : base(Constants.ProviderId, DefaultAcceptedEntityTypes)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider) => this.Accepts(config);

        private IEnumerable<EntityType> Accepts(IDictionary<string, object> config)
            => Accepts(new PermIdExternalSearchJobData(config));

        private IEnumerable<EntityType> Accepts(PermIdExternalSearchJobData config)
        {
            if (!string.IsNullOrWhiteSpace(config.AcceptedEntityType))
            {
                // If configured, only accept the configured entity types
                return new EntityType[] { config.AcceptedEntityType };
            }

            // Fallback to default accepted entity types
            return DefaultAcceptedEntityTypes;
        }

        private bool Accepts(PermIdExternalSearchJobData config, EntityType entityTypeToEvaluate)
        {
            var configurableAcceptedEntityTypes = this.Accepts(config).ToArray();

            return configurableAcceptedEntityTypes.Any(entityTypeToEvaluate.Is);
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
            => InternalBuildQueries(context, request, new PermIdExternalSearchJobData(config));

        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, PermIdExternalSearchJobData config)
        {
            if (!this.Accepts(config, request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<PermIdSocialResponse>(this).ToList();

            Func<string, bool> existingDataFilter   = value => existingResults.Any(r => string.Equals(r.Data.OrganizationName.First(), value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> nameFilter           = value => OrganizationFilters.NameFilter(context, value) || existingResults.Any(r => string.Equals(r.Data.OrganizationName.First(), value, StringComparison.InvariantCultureIgnoreCase));

            // Query Input
            var entityType       = request.EntityMetaData.EntityType;

            var organizationName = GetValue(request, config.ToDictionary(), Constants.KeyName.OrganizationName, Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName);


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

        private static HashSet<string> GetValue(IExternalSearchRequest request, IDictionary<string, object> config, string keyName, VocabularyKey defaultKey)
        {
            HashSet<string> value;
            if (config.TryGetValue(keyName, out var customVocabKey) && !string.IsNullOrWhiteSpace(customVocabKey?.ToString()))
            {
                value = request.QueryParameters.GetValue<string, HashSet<string>>(customVocabKey.ToString(), new HashSet<string>());
            }
            else
            {
                value = request.QueryParameters.GetValue(defaultKey, new HashSet<string>());
            }

            return value;
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            var jobData = new PermIdExternalSearchJobData(config);

            var name     = query.QueryParameters[ExternalSearchQueryParameter.Name].FirstOrDefault();
            var idList   = new List<string>();
            var apiToken = jobData.ApiToken;

            if (string.IsNullOrEmpty(apiToken))
                throw new InvalidOperationException("PermId ApiToken has not been configured");

            if (string.IsNullOrEmpty(name))
                yield break;

            var searchClient = new RestClient(" https://api-eit.refinitiv.com/permid");
            var socialClient = new RestClient("https://permid.org/api/mdaas/getEntityById/");

            if (!string.IsNullOrEmpty(name))
            {
                var searchResult = RequestWrapper<PermIdSearchResponse>(searchClient, "search?q=" + name, apiToken);

                foreach (var res in searchResult.Result.Organizations.Entities)
                {
                    idList.Add(res.Id.Split('-').Last());
                }
            }

            foreach (var permId in idList)
            {
                var socialResult = RequestWrapper<PermIdSocialResponse>(socialClient, permId, apiToken);

                if (socialResult != null)
                {
                    yield return new ExternalSearchQueryResult<PermIdSocialResponse>(query, socialResult);
                    yield break;
                }
            }
        }

        private static T RequestWrapper<T>(IRestClient client, string parameter, string apiKey)
        {
            T retval = default(T);

            ActionExtensions.ExecuteWithRetry(() =>
            {
                var request = new RestRequest(parameter, Method.GET);

                request.AddHeader("X-AG-Access-Token", apiKey);

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

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var organizationClue = new Clue(request.EntityMetaData.OriginEntityCode, context.Organization);
            organizationClue.Data.EntityData.Codes.Add(request.EntityMetaData.Codes.First());

            this.PopulateMetadata(organizationClue.Data.EntityData, result.As<PermIdSocialResponse>(), request);

            yield return organizationClue;

            if (result.As<PermIdSocialResponse>().Data.AdditionalInfo != null)
            {
                //foreach (var person in result.As<PermIdSocialResponse>().Data.AdditionalInfo)
                //{
                //    var personCode = this.GetPersonEntityCode(person, request);
                //    var personClue = new Clue(personCode, context.Organization);

                //    this.PopulatePersonMetadata(personClue.Data.EntityData, person, request);

                //    var personToOrganizationEdge = new EntityEdge(new EntityReference(personCode), new EntityReference(organizationCode), EntityEdgeType.WorksFor);
                //    personClue.Data.EntityData.OutgoingEdges.Add(personToOrganizationEdge);

                //    yield return personClue;
                //}
            }
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var resultItem = result.As<PermIdSocialResponse>();

            if (resultItem == null)
                return null;

            return this.CreateMetadata(resultItem, request);
        }

        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return null;
        }

        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<PermIdSocialResponse> resultItem, IExternalSearchRequest request)
        {
            if (resultItem == null)
                throw new ArgumentNullException(nameof(resultItem));

            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem, request);

            return metadata;
        }

        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<PermIdSocialResponse> resultItem, IExternalSearchRequest request)
        {
            if (resultItem == null)
                throw new ArgumentNullException(nameof(resultItem));

            var data = resultItem.Data;

            metadata.EntityType  = request.EntityMetaData.EntityType;
            metadata.Name        = request.EntityMetaData.Name;
            metadata.CreatedDate = resultItem.CreatedDate;

            metadata.OriginEntityCode = request.EntityMetaData.OriginEntityCode;

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

        private void PopulatePersonMetadata(IEntityMetadata metadata, AssociatedPerson person, IExternalSearchRequest request)
        {
            metadata.EntityType = request.EntityMetaData.EntityType;

            metadata.Name             = request.EntityMetaData.Name;
            metadata.OriginEntityCode = request.EntityMetaData.OriginEntityCode;

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

        // Since this is a configurable external search provider, theses methods should never be called
        public override bool Accepts(EntityType entityType) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query) => throw new NotSupportedException();
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();

        /**********************************************************************************************************
         * PROPERTIES
         **********************************************************************************************************/

        public string Icon { get; } = Constants.Icon;
        public string Domain { get; } = Constants.Domain;
        public string About { get; } = Constants.About;
        public AuthMethods AuthMethods { get; } = Constants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = Constants.Properties;
        public Guide Guide { get; } = Constants.Guide;
        public IntegrationType Type { get; } = Constants.IntegrationType;
    }
}
