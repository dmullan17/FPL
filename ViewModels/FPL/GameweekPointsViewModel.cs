using FPL.Models;
using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels.FPL
{
    public class GameweekPointsViewModel : FPL
    {
        public GWTeam GWTeam { get; set; }
        public int GWPoints { get; set; }
        public int GameweekId { get; set; }
        public EventStatus EventStatus { get; set; }

    }
}
