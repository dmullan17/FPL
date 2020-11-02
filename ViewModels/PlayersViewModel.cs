using FPL.Models;
using System.Collections.Generic;

namespace FPL.ViewModels
{
    public class PlayersViewModel
    {
        public List<Player> AllPlayers { get; set; }

        public Player Player { get; set; }

        public List<PlayerPosition> Positions { get; set; }

        public int TotalPlayerCount { get; set; }
    }
}