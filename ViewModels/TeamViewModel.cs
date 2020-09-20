using FPL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels
{
    public class TeamViewModel
    {
        public List<Team> AllTeams { get; set; }

        public Team Team { get; set; }
    }
}
