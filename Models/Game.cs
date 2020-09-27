using System;
using System.Collections.Generic;

namespace FPL.Models
{
    public class Game
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public int code { get; set; } 
        public int? Event { get; set; } 
        public bool finished { get; set; } 
        public bool finished_provisional { get; set; } 
        public int id { get; set; } 
        public DateTime? kickoff_time { get; set; } 
        public int minutes { get; set; } 
        public bool provisional_start_time { get; set; } 
        public bool? started { get; set; } 
        public int team_a { get; set; } 
        public string team_a_name { get; set; } 
        public int? team_a_score { get; set; } 
        public int team_h { get; set; } 
        public string team_h_name { get; set; } 
        public int? team_h_score { get; set; } 
        public List<Stat> stats { get; set; } 
        public int team_h_difficulty { get; set; } 
        public int team_a_difficulty { get; set; }
        public bool? is_half_time { get; set; }
    }
}