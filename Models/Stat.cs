using System.Collections.Generic;

namespace FPL.Models
{
    public class Stat
    {
        public string identifier { get; set; }
        public List<PlayerStat> a { get; set; }
        public List<PlayerStat> h { get; set; }
    }
}