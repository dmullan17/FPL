using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class BasicChip
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("event")]
        public int Event { get; set; }
    }
}
