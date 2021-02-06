using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Classic
    {
        public Classic()
        {
            Standings = new Standings();
        }
        public List<Pick> Captains { get; set; } = new List<Pick>();
        public List<Pick> Players { get; set; } = new List<Pick>();
        public List<PlayerTally> CaptainsTally { get; set; } = new List<PlayerTally>();
        public List<PlayerTally> PlayersTally { get; set; } = new List<PlayerTally>();
        public int PlayerCount { get; set; }
        public Standings Standings { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public DateTime created { get; set; }
        public bool closed { get; set; }
        public object rank { get; set; }
        public object max_entries { get; set; }
        public string league_type { get; set; }
        public string scoring { get; set; }
        public int? admin_entry { get; set; }
        public int start_event { get; set; }
        public int entry_rank { get; set; }
        public int entry_last_rank { get; set; }
        public bool entry_can_leave { get; set; }
        public bool entry_can_admin { get; set; }
        public bool entry_can_invite { get; set; }
    }
}
