using FPL.Models;
using System.Collections.Generic;

namespace FPL.ViewModels
{
    public class PlayerViewModel
    {
        public List<Player> AllPlayers { get; set; }

        public Player Player { get; set; }

        public int TotalPlayerCount { get; set; }
    }
}