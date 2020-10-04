using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Leagues
    {
        public List<Classic> classic { get; set; }
        public List<object> h2h { get; set; }
        public Cup cup { get; set; }
    }
}
