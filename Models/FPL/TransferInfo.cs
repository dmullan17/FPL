using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class TransferInfo
    {
        public int cost { get; set; }
        public string status { get; set; }
        public int limit { get; set; }
        public int made { get; set; }
        public int bank { get; set; }
        public int value { get; set; }
    }
}
