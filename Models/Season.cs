using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class Season
    {
        [JsonProperty("season_name")]
        public string SeasonName { get; set; }

        [JsonProperty("total_points")]
        public int TotalPoints { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
}
