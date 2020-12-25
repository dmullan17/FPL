using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Result
    {
        public Result()
        {
            GwPicks = new List<Pick>();
        }
        public List<Pick> GwPicks { get; set; }
        public int id { get; set; }
        public int event_total { get; set; }
        public string player_name { get; set; }
        public int rank { get; set; }
        public int last_rank { get; set; }
        public int rank_sort { get; set; }
        public int total { get; set; }
        public int entry { get; set; }
        public string entry_name { get; set; }
    }
}
