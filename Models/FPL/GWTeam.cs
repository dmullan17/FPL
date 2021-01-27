using FPL.Models.GWPlayerStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class GWTeam
    {
        public GWTeam()
        {
            picks = new List<Pick>();
            automatic_subs = new List<AutomaticSub>();
            ActiveChips = new List<string>();
        }
        //public Player player { get; set; }
        //public GWPlayer GWPlayer { get; set; }
        public List<AutomaticSub> automatic_subs { get; set; }

        public List<Pick> picks { get; set; }

        public List<string> ActiveChips { get; set; }

        public EntryHistory EntryHistory { get; set; }
    }
}
