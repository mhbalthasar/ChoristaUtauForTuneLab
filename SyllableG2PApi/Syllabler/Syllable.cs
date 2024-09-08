using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableG2PApi.Syllabler
{
    public abstract class Syllabler
    {
        public struct Note
        {
            /// <summary>
            /// Lyric of the note, the part of lyric that is not enclosed in "[]".
            /// Example: if lyric on note is "read". The lyric is "read".
            /// Example: if lyric on note is "read[r iy d]". The lyric is "read".
            /// </summary>
            public string lyric;

            /// <summary>
            /// Phonetic hint,
            /// Example: if lyric on note is "read". The hint is null.
            /// Example: if lyric on note is "read[r iy d]". The hint is "r iy d".
            /// </summary>
            public string phoneme;

            /// <summary>
            /// Music tone of note. C4 = 60.
            /// </summary>
            public int pitchNumber;

            /// <summary>
            /// Position of note in project, measured in ticks.
            /// Use timeAxis to convert between ticks and milliseconds .
            /// </summary>
            public int position;

            /// <summary>
            /// Duration of note measured in ticks.
            /// Use timeAxis to convert between ticks and milliseconds .
            /// </summary>
            public int duration;

            public override string ToString() => $"\"{lyric}\" pos:{position}";
        }

        public struct Syllable
        {
            /// <summary>
            /// vowel from previous syllable for VC
            /// </summary>
            public string prevV;
            /// <summary>
            /// CCs, may be empty
            /// </summary>
            public string[] cc;
            /// <summary>
            /// "base" note. May not actually be vowel, if only consonants way provided
            /// </summary>
            public string v;
            /// <summary>
            /// Start position for vowel. All VC CC goes before this position
            /// </summary>
            public int position;
            /// <summary>
            /// previous note duration, i.e. this is container for VC and CC notes
            /// </summary>
            public int duration;
            /// <summary>
            /// Tone for VC and CC
            /// </summary>
            public int pitchNumber;
            /// <summary>
            /// tone for base "vowel" phoneme
            /// </summary>
            public int vowelTone;

            /// <summary>
            /// 0 if no consonants are taken from previous word;
            /// 1 means first one is taken from previous word, etc.
            /// </summary>
            public int prevWordConsonantsCount;

            /// <summary>
            /// If true, you may use alias extension instead of VV, by putting the phoneme as null if vowels match. 
            /// If you do this when canAliasBeExtended == false, the note will produce no phoneme and there will be a break.
            /// Use CanMakeAliasExtension() to pass all checks if alias extension is possible
            /// </summary>
            public bool canAliasBeExtended;

            // helpers
            public bool IsStartingV => prevV == "" && cc.Length == 0;
            public bool IsVV => prevV != "" && cc.Length == 0;

            public bool IsStartingCV => prevV == "" && cc.Length > 0;
            public bool IsVCV => prevV != "" && cc.Length > 0;

            public bool IsStartingCVWithOneConsonant => prevV == "" && cc.Length == 1;
            public bool IsVCVWithOneConsonant => prevV != "" && cc.Length == 1;

            public bool IsStartingCVWithMoreThanOneConsonant => prevV == "" && cc.Length > 1;
            public bool IsVCVWithMoreThanOneConsonant => prevV != "" && cc.Length > 1;

            public string[] PreviousWordCc => cc.Take(prevWordConsonantsCount).ToArray();
            public string[] CurrentWordCc => cc.Skip(prevWordConsonantsCount).ToArray();

            public override string ToString()
            {
                return $"({prevV}) {(cc != null ? string.Join(" ", cc) : "")} {v}";
            }
        }

        public struct Ending
        {
            /// <summary>
            /// vowel from the last syllable to make VC
            /// </summary>
            public string prevV;
            /// <summary>
            ///  actuall CC at the ending
            /// </summary>
            public string[] cc;
            /// <summary>
            /// last note position + duration, all phonemes must be less than this
            /// </summary>
            public int position;
            /// <summary>
            /// last syllable length, max container for all VC CC C-
            /// </summary>
            public int duration;
            /// <summary>
            /// the tone from last syllable, for all ending phonemes
            /// </summary>
            public int pitchNumber;

            // helpers
            public bool IsEndingV => cc.Length == 0;
            public bool IsEndingVC => cc.Length > 0;
            public bool IsEndingVCWithOneConsonant => cc.Length == 1;
            public bool IsEndingVCWithMoreThanOneConsonant => cc.Length > 1;

            public override string ToString()
            {
                return $"({prevV}) {(cc != null ? string.Join(" ", cc) : "")}";
            }
        }
    }
}
