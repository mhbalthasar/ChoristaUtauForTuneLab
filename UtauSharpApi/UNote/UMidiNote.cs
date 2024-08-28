using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UTask;

namespace UtauSharpApi.UNote
{
    public class UMidiNote
    {
        public UMidiPart Parent { get; private set; }
        public UMidiNote(UMidiPart parent) { Parent = parent; }
        public List<UPhonemeNote> PhonemeNotes { get; set; } = new List<UPhonemeNote>();
        public string Lyric { get; set; } = "";
        public double StartMSec { get; set; } = 0;
        public double DurationMSec { get; set; } = 0;
        public int NoteNumber { get; set; } = 60;
        public string Flags { get; set; } = "";
        public int Velocity { get; set; } = 100;
        public object? ObjectTag { get; set; } = null;//BringInformations

        public List<string> Phonemes { get {
                return PhonemeNotes.Select(i => i.ToString()).ToList();
            }
            set {
                PhonemeNotes=value.Select(i => new UPhonemeNote(this, i)).ToList();
            } 
        }
    }
}
