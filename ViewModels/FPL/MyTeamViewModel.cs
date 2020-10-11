using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels.FPL
{
    public class MyTeamViewModel : FPL
    {
        public List<Pick> Picks { get; set; }

        public List<Chip> Chips { get; set; }

        public TransferInfo TransferInfo { get; set; }
    }
}
