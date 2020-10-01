using System.Collections.Generic;

namespace FPL.Models.GWPlayerStats
{
    public class GWPlayer
    {
        public int id { get; set; }
        public Player player { get; set; } 
        public Team team { get; set; }
        public GWPlayerStats stats { get; set; } 
        public List<GWPlayerStatsExplain> explain { get; set; } 
    }
    
}