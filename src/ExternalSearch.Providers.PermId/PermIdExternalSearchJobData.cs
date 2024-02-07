using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.PermId
{
    public class PermIdExternalSearchJobData : CrawlJobData
    {
        public PermIdExternalSearchJobData(IDictionary<string, object> configuration)
        {
            ApiToken = GetValue<string>(configuration, Constants.KeyName.ApiToken);
            AcceptedEntityType = GetValue<string>(configuration, Constants.KeyName.AcceptedEntityType);
            OrganizationName = GetValue<string>(configuration, Constants.KeyName.OrganizationName);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { Constants.KeyName.ApiToken, ApiToken },
                { Constants.KeyName.AcceptedEntityType, AcceptedEntityType },
                { Constants.KeyName.OrganizationName, OrganizationName }
            };
        }
        public string ApiToken { get; set; }
        public string AcceptedEntityType { get; set; }
        public string OrganizationName { get; set; }
    }
}
