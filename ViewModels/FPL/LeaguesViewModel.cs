using FPL.Models;
using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels.FPL
{
    public class LeaguesViewModel : BaseViewModel
    {
        public List<Classic> ClassicLeagues { get; set; }
        public List<object> H2HLeagues { get; set; }
        public Cup Cup { get; set; }
        public bool IsEventLive { get; set; }
        public int CurrentGwId { get; set; }
        public Classic SelectedLeague { get; set; }
    }
}
