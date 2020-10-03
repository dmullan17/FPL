using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models.FPL
{
    public class Chip
    {
        public string status_for_entry { get; set; }
        public List<int> played_by_entry { get; set; }
        public string name { get; set; }
        public int number { get; set; }
        public int start_event { get; set; }
        public int stop_event { get; set; }
        public string chip_type { get; set; }
    }
}
