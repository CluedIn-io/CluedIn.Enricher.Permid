// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdPersonVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier person vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.PermId.Vocabularies
{
    public class PermIdPersonVocabulary : PermIdVocabulary
    {   
        public PermIdPersonVocabulary()
        {
            this.VocabularyName = "PermId Person";
            this.KeyPrefix      = "permId.person";
            this.KeySeparator   = ".";
            this.Grouping       = CluedIn.Core.Data.EntityType.Infrastructure.User;

            this.AddGroup("PermId Person Details", group =>
            {
                this.PreferredName                      = group.Add(new VocabularyKey("preferredName"));
                this.HonorificPrefix                    = group.Add(new VocabularyKey("honorificPrefix"));
                this.GivenName                          = group.Add(new VocabularyKey("givenName"));
                this.MiddleName                         = group.Add(new VocabularyKey("middleName"));
                this.FamilyName                         = group.Add(new VocabularyKey("familyName"));
                this.EntityType                         = group.Add(new VocabularyKey("entityType"));
                this.PersonUrl                          = group.Add(new VocabularyKey("personUrl",                      VocabularyKeyDataType.Uri));
            });

            this.AddGroup("Position Information", group =>
            {
                this.HasReportedTitlePerson             = group.Add(new VocabularyKey("hasReportedTitlePerson"));
                this.PositionType                       = group.Add(new VocabularyKey("positionType"));
                this.PositionRank                       = group.Add(new VocabularyKey("positionRank",                   VocabularyKeyDataType.Integer, VocabularyKeyVisibility.Hidden));
                this.PositionStartDate                  = group.Add(new VocabularyKey("positionStartDate"));
                this.PositionUrl                        = group.Add(new VocabularyKey("positionUrl",                    VocabularyKeyDataType.Uri));
            });

            this.AddMapping(this.GivenName,                 CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.FirstName);
            this.AddMapping(this.MiddleName,                CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.MiddleName);
            this.AddMapping(this.FamilyName,                CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.LastName);
            this.AddMapping(this.HasReportedTitlePerson,    CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.JobTitle);
        }

        public VocabularyKey PersonUrl { get; set; }
        public VocabularyKey HonorificPrefix { get; set; }
        public VocabularyKey GivenName { get; set; }
        public VocabularyKey MiddleName { get; set; }
        public VocabularyKey FamilyName { get; set; }
        public VocabularyKey HasReportedTitlePerson { get; set; }
        public VocabularyKey EntityType { get; set; }
        public VocabularyKey PositionUrl { get; set; }
        public VocabularyKey PositionType { get; set; }
        public VocabularyKey PositionRank { get; set; }
        public VocabularyKey PositionStartDate { get; set; }
        public VocabularyKey PreferredName { get; set; }
    }
}