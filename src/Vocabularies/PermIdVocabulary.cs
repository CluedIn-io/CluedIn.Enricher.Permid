// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.PermId.Vocabularies
{
    /// <summary>A permission identifier vocabulary.</summary>
    /// <seealso cref="T:CluedIn.Core.Data.Vocabularies.SimpleVocabulary"/>
    public abstract class PermIdVocabulary : SimpleVocabulary
    {
        protected PermIdVocabulary()
        {
            this.VocabularyName = "PermId";
            this.KeyPrefix      = "permId";
            this.KeySeparator   = ".";
            this.Grouping       = CluedIn.Core.Data.EntityType.Unknown;

            this.PermId = this.Add(new VocabularyKey("permId", VocabularyKeyVisibility.Hidden));
        }
        
        public VocabularyKey PermId { get; set; }
    }
}