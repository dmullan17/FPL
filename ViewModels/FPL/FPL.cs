using FPL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FPLTeam = FPL.Models.FPL.Team;

namespace FPL.ViewModels.FPL
{
    public class FPL
    {
        public FPLTeam Team { get; set; }

        public List<PlayerPosition> Positions { get; set; }

        public int TotalPoints { get; set; }
    }
}
