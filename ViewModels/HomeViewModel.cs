using FPL.Models;
using FPL.Models.GWPlayerStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels
{
    public class HomeViewModel
    {
        public GameWeek CurrentGameweek { get; set; }
        public List<GWPlayer> Players { get; set; }
        public List<Game> GWGames { get; set; } = new List<Game>();
    
    }
}
