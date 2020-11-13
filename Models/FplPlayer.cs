using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class FplPlayer
    {
        public string date_of_birth { get; set; }
        public bool dirty { get; set; }
        public string first_name { get; set; }
        public string gender { get; set; }
        public int id { get; set; }
        public string last_name { get; set; }
        public int region { get; set; }
        public string email { get; set; }
        public int entry { get; set; }
        public bool entry_email { get; set; }
    }
}
