﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.PermId.Models
{
    public class Instrument
    {
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("num")]
        public long Num { get; set; }

        [JsonProperty("entities")]
        public List<InstrumentEntity> Entities { get; set; }
    }
}