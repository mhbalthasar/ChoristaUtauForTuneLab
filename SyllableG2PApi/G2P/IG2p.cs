using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableG2PApi.G2P
{
    public interface IG2p
    {
        bool IsValidSymbol(string symbol);
        bool IsVowel(string symbol);

        /// <summary>
        /// Returns true if the symbol is a semivowel or liquid phoneme, like y, w, l, r in English.
        /// </summary>
        bool IsGlide(string symbol);

        /// <summary>
        /// Produces a list of phonemes from grapheme.
        /// </summary>
        string[] Query(string grapheme);

        /// <summary>
        /// Produces a list of phonemes from hint, removing invalid symbols.
        /// </summary>
        string[] UnpackHint(string hint, char separator = ' ');
    }
}
