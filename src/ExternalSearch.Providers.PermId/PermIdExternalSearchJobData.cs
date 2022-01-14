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
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { Constants.KeyName.ApiToken, ApiToken }
            };
        }
        public string ApiToken { get; set; }
    }
}
