﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UNote;
using UtauSharpApi.UTask;

namespace UtauSharpApi.UTask
{
    public class UTaskProject
    {
        private static int MinTickSpacing=5;//UTAU CONTROL SPACING IS 5;
        public static int FormatTick(int Tick) { return ((int)((double)Tick / (double)MinTickSpacing)) * MinTickSpacing; }

        public UMidiPart Part = new UMidiPart();
    }
}