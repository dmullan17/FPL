using FPL.Models;
using System.Collections.Generic;

namespace FPL.ViewModels
{
    public class FixturesViewModel
    {
        public List<Game> Fixtures { get; set; }

        public Game Game { get; set; }
    }
}