using FPL.Models;
using System.Collections.Generic;

namespace FPL.ViewModels
{
    public class FixturesViewModel
    {
        public List<Game> Fixtures { get; set; }

        public List<Game> LiveGames { get; set; }

        public int CurrentGameweekId { get; set; }

        public GameWeek CurrentGameweek { get; set; }

        public Game Game { get; set; }
    }
}