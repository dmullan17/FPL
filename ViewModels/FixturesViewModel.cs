using FPL.Models;
using FPL.Models.GWPlayerStats;
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

        public Game LiveGame { get; set; }

        public List<GWPlayer> GWPlayersStats { get; set; }
    }
}