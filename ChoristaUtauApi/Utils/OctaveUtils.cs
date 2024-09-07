using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.Utils
{
    public class OctaveUtils
    {
        public static readonly Dictionary<string, int> NameInOctave = new Dictionary<string, int> {
            { "C", 0 }, { "C#", 1 }, { "Db", 1 },
            { "D", 2 }, { "D#", 3 }, { "Eb", 3 },
            { "E", 4 },
            { "F", 5 }, { "F#", 6 }, { "Gb", 6 },
            { "G", 7 }, { "G#", 8 }, { "Ab", 8 },
            { "A", 9 }, { "A#", 10 }, { "Bb", 10 },
            { "B", 11 },
        };
        public static int Str2NoteNumber(string octave)
        {
            octave = octave.Trim().ToUpper();
            if(octave.Length>1 && octave[0]>='A' && octave[0]<='G')
            {
                int oH = 0;
                int oC = 0;
                if(octave[1]=='#')
                {
                    oH = NameInOctave[octave[0] + "#"];
                    oC = int.Parse(octave.Substring(2));
                }
                else if (octave[1] == 'b')
                {
                    oH = NameInOctave[octave[0] + "b"];
                    oC = int.Parse(octave.Substring(2));
                }
                else
                {
                    oH = NameInOctave[octave[0]+""];
                    oC = int.Parse(octave.Substring(1));
                }
                return oC * 12 + oH;
            }
            return -1;
        }

        public static string NoteNumber2Str(int NoteNumber)
        {
            int oC = (int)(NoteNumber / 12.0);
            int oH = NoteNumber - oC * 12;
            string oK = NameInOctave.Where(k => (k.Value == oH && !k.Key.EndsWith("b"))).First().Key;
            return oK + oC.ToString();
        }
    }
}
