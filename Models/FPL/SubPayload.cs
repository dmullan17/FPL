using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class SubPayload
    {
        public object chip { get; set; }
        public List<SubPick> picks { get; set; } = new List<SubPick>();
    }
}
