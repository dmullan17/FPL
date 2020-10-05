using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels
{
    public class FPLViewModel
    {
        public GWTeam GWTeam { get; set; }
        public List<Chip> chips { get; set; }
        public Transfers transfers { get; set; }

        public Team Team { get; set; }
        public int GWPoints { get; set; }
        public int TotalPoints { get; set; }
        public int GameweekId { get; set;  } 
    }
}
