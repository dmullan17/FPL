using FPL.Models.GWPlayerStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Pick
    {
        public Pick()
        {
            HadSinceGW = 1;
            IsNewTransfer = false;
        }

        public Player player { get; set; }
        public GWPlayer GWPlayer { get; set; }
        public List<Game> GWGames { get; set; } = new List<Game>();
        public string GWOppositionName { get; set; }

        public List<Transfer> Transfer { get; set; }

        public int TotalPointsAccumulatedForTeam { get; set; }
        public int HadSinceGW { get; set; }
        public int GWOnTeam { get; set; }
        public float PPGOnTeam { get; set; }
        public int NetProfitOnTransfer { get; set; }
        public bool IsNewTransfer { get; set; }
        public int element { get; set; }
        public int position { get; set; }
        public int selling_price { get; set; }
        public int multiplier { get; set; }
        public int purchase_price { get; set; }
        public bool is_captain { get; set; }
        public bool is_vice_captain { get; set; }

    }
}
