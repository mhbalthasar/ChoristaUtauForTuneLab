using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtauSharpApi.UNote
{
    public class UPhonemeNote
    {
        public UMidiNote Parent { get; private set; }
        public UPhonemeNote(UMidiNote note)
        {
            Parent = note;
        }
        public UPhonemeNote(UMidiNote note, string symbol)
        {
            Parent = note;
            Symbol = symbol;
        }

        public string Symbol { get; private set; } = "R";
        public double SymbolMSec { get; set; } = -1;

        public override string ToString()
        {
            return Symbol;
        }
    }
}
