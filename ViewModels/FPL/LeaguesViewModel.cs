using FPL.Models;
using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels.FPL
{
    public class LeaguesViewModel
    {
        public List<Classic> ClassicLeagues { get; set; }
        public List<object> H2HLeagues { get; set; }
        public Cup Cup { get; set; }
        public bool IsEventLive { get; set; }
        //public int id { get; set; }
        //public DateTime joined_time { get; set; }
        //public int started_event { get; set; }
        //public object favourite_team { get; set; }
        //public string player_first_name { get; set; }
        //public string player_last_name { get; set; }
        //public int player_region_id { get; set; }
        //public string player_region_name { get; set; }
        //public string player_region_iso_code_short { get; set; }
        //public string player_region_iso_code_long { get; set; }
        //public int summary_overall_points { get; set; }
        //public int summary_overall_rank { get; set; }
        //public int summary_event_points { get; set; }
        //public int summary_event_rank { get; set; }
        //public int current_event { get; set; }
        //public Leagues leagues { get; set; }
        //public string name { get; set; }
        //public string kit { get; set; }
        //public int last_deadline_bank { get; set; }
        //public int last_deadline_value { get; set; }
        //public int last_deadline_total_transfers { get; set; }
    }
}
