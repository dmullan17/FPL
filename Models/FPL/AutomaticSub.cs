using FPL.Models.GWPlayerStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class AutomaticSub
    {
        public Player player { get; set; }
        public GWPlayer GWPlayer { get; set; }
        public List<Game> GWGames { get; set; } = new List<Game>();
        public string GWOppositionName { get; set; }
        public List<Transfer> Transfer { get; set; }
        public int entry { get; set; }
        public int element_in { get; set; }
        public int element_out { get; set; }
        public int @event { get; set; }
    }
}
