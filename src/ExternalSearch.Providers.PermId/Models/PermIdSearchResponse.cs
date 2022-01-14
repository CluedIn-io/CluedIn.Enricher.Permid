// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermIdSearchResponse.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the permission identifier search response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
	public class PermIdSearchResponse
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
    }
}
