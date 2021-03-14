using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class PlayerTally
    {
        public Pick Pick { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string Ownership { get; set; }
        public string StartingOwnership { get; set; }
        public string CaptainSelection { get; set; }
        public string BenchOwnership { get; internal set; }
        public string TransferredOut { get; internal set; }
        public string TransferredIn { get; internal set; }
    }
}
