using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;

namespace CluedIn.ExternalSearch.Providers.PermId
{
    public static class Constants
    {
        public const string ComponentName = "PermId";
        public const string ProviderName = "PermId";
        public static readonly Guid ProviderId = Core.Constants.ExternalSearchProviders.PermId;

        public struct KeyName
        {
            public const string ApiToken = "apiToken";
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string OrganizationName = "organizationName";
        }

        public static string About { get; set; } = "PermID is an enricher using permanent and universal identifiers where underlying attributes capture the context of the identity they each represent";
        public static string Icon { get; set; } = "Resources.permid.jpg";
        public static string Domain { get; set; } = "https://permid.org/";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = "Api Key",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.ApiToken,
                    Help = "The key to authenticate access to the PermID API."
                },
                new Control()
                {
                    DisplayName = "Accepted Entity Type",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = "The entity type that defines the golden records you want to enrich (e.g., /Organization)."
                },
                new Control()
                {
                    DisplayName = "Organization Name Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.OrganizationName,
                    Help = "The vocabulary key that contains the names of companies you want to enrich (e.g., organization.name)."
                }
            }
        };

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
