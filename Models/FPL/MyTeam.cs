using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class MyTeam
    {
        public List<Pick> picks { get; set; }
        public List<Chip> chips { get; set; }
        public TransferInfo transfers { get; set; }
    }
}
