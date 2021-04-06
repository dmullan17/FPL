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
        public GameweekPointsViewModel()
        {
            IsLive = false;
        }
        public GWTeam GWTeam { get; set; }
        public int GWPoints { get; set; }
        public int GameweekId { get; set; }
        public int CurrentGameweekId { get; set; }
        public EventStatus EventStatus { get; set; }
        public EntryHistory EntryHistory { get; set; }
        public CompleteEntryHistory CompleteEntryHistory { get; set; }
        public bool IsLive { get; set; }
        public GameWeek GameWeek { get; set; }
        public List<GameWeek> AllStartedGameWeeks { get; set; } = new List<GameWeek>();

    }
}
