using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Standings
    {
        public Standings()
        {
            results = new List<Result>();
        }

        public bool has_next { get; set; }
        public int page { get; set; }
        public List<Result> results { get; set; }

    }
}
