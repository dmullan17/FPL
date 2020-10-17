using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.GWPlayerStats
{
    public class RootGWDreamTeam
    {
        public GWTopPlayer top_player { get; set; }
        public List<GWDreamTeam> team { get; set; }
    }
}
