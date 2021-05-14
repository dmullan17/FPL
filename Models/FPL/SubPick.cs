using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class SubPick
    {
        public int element { get; set; }
        public int element_type { get; set; }
        public int position { get; set; }
        public int multiplier { get; set; }
        public bool is_captain { get; set; }
        public bool is_vice_captain { get; set; }

    }
}
