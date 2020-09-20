using FPL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels
{
    public class GameWeekViewModel
    {
        public List<GameWeek> Gameweeks { get; set; }

        public GameWeek CurrentGameweek { get; set; }
    }
}
