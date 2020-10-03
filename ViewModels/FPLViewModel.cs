﻿using FPL.Models.FPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.ViewModels
{
    public class FPLViewModel
    {
        public List<Pick> picks { get; set; }
        public List<Chip> chips { get; set; }
        public Transfers transfers { get; set; }
    }
}