using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class EntryHistory
    {
        public int GwRankPercentile { get; set; }
        public int TotalRankPercentile { get; set; }
        public int TotalPlayers { get; set; }
        public int @event { get; set; }
        public int points { get; set; }
        public int total_points { get; set; }
        public int? rank { get; set; }
        public int? rank_sort { get; set; }
        public int? overall_rank { get; set; }
        public int? LastEventOverallRank { get; set; }
        public int bank { get; set; }
        public int value { get; set; }
        public int event_transfers { get; set; }
        public int event_transfers_cost { get; set; }
        public int points_on_bench { get; set; }
    }
}
