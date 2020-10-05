using FPL.Models.GWPlayerStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class BasePick
    {
        public Player player { get; set; }
        public GWPlayer GWPlayer { get; set; }
    }
}
