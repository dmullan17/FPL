using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Result
    {
        public CompleteEntryHistory CompleteEntryHistory { get; set; } = new CompleteEntryHistory();
        public GWTeam GWTeam { get; set; }
        public List<int> Last5GwPoints { get; set; } = new List<int>();
        public int id { get; set; }
        public int event_total { get; set; }
        public string player_name { get; set; }
        public int rank { get; set; }
        public int last_rank { get; set; }

        public int LiveRank { get; set; }
        public int PlayersYetToPlay { get; set; }
        public int rank_sort { get; set; }
        public int total { get; set; }
        public int entry { get; set; }
        public string entry_name { get; set; }
    }
}
