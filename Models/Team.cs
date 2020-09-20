using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class Team
    {
        public int code { get; set; }
        public int draw { get; set; }
        public object form { get; set; }
        public int id { get; set; }
        public int loss { get; set; }
        public string name { get; set; }
        public int played { get; set; }
        public int points { get; set; }
        public int position { get; set; }
        public string short_name { get; set; }
        public int strength { get; set; }
        public object team_division { get; set; }
        public bool unavailable { get; set; }
        public int win { get; set; }
        public int strength_overall_home { get; set; }
        public int strength_overall_away { get; set; }
        public int strength_attack_home { get; set; }
        public int strength_attack_away { get; set; }
        public int strength_defence_home { get; set; }
        public int strength_defence_away { get; set; }
        public int pulse_id { get; set; }
    }
}
