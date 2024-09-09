using System.Collections.Generic;
using System.Text;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Classic {

    public class OtoSet {
        public string File;
        public string Name;
        public List<Oto> Otos = new List<Oto>();

        public override string ToString() {
            return Name;
        }
    }

    public class Oto {
        public string Alias;
        public string Phonetic;
        public string Wav;

        // Wav layout:
        // |-offset-|-consonant-(fixed)-|-stretched-|-cutoff-|
        // |        |-preutter-----|
        // |        |-overlap-|
        // Note position:
        // ... ----------prev-note-|-this-note-- ...
        // Phoneme overlap:
        // ... --prev-phoneme-\
        //          /-this-phoneme-------------- ...

        // Length of left offset.
        public double Offset;
        // Length of unstretched consonant in wav, AKA fixed.
        public double Consonant;
        // Length of right cutoff, AKA end blank. If negative, length of (consonant + stretched). 
        public double Cutoff;
        // Length before note start, usually within consonant range.
        public double Preutter;
        // Length overlap with previous note, usually within consonant range.
        public double Overlap;

        public bool IsValid;
        public string Error;
        //public FileTrace FileTrace;

        public override string ToString() {
            return Alias;
        }
    }
}
