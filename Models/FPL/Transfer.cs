using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Transfer
    {
        public Player PlayerIn { get; set; }
        public Player PlayerOut { get; set; }
        public int element_in { get; set; }
        public int element_in_cost { get; set; }
        public int element_out { get; set; }
        public int element_out_cost { get; set; }
        public int entry { get; set; }
        public int @event { get; set; }
        public DateTime time { get; set; }
    }
}
