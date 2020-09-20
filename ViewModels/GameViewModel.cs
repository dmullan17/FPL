using FPL.Models;
using System.Collections.Generic;

namespace FPL.ViewModels
{
    public class GameViewModel
    {
        public List<Game> AllGames { get; set; }

        public Game Game { get; set; }
        public List<Stat> GameStats { get; set; }

        public int TotalGameCount { get; set; }
    }
}