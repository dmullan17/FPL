using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class AutomaticSub : BasePick
    {
        public int entry { get; set; }
        public int element_in { get; set; }
        public int element_out { get; set; }
        public int @event { get; set; }
    }
}
