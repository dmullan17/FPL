using System.Collections.Generic;

namespace FPL.Models
{
    public class Stat
    {
        public string identifier { get; set; }
        public List<Away> a { get; set; }
        public List<Home> h { get; set; }
    }
}