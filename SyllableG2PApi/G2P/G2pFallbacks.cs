using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableG2PApi.G2P
{
    public class G2pFallbacks : IG2p
    {
        IG2p[] dictionaries;

        public G2pFallbacks(IG2p[] dictionaries)
        {
            this.dictionaries = dictionaries;
        }

        public bool IsValidSymbol(string symbol)
        {
            foreach (var dict in dictionaries)
            {
                if (dict.IsValidSymbol(symbol))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsVowel(string symbol)
        {
            foreach (var dict in dictionaries)
            {
                if (dict.IsValidSymbol(symbol))
                {
                    return dict.IsVowel(symbol);
                }
            }
            return false;
        }

        public bool IsGlide(string symbol)
        {
            foreach (var dict in dictionaries)
            {
                if (dict.IsValidSymbol(symbol))
                {
                    return dict.IsGlide(symbol);
                }
            }
            return false;
        }

        public string[] Query(string grapheme)
        {
            foreach (var dict in dictionaries)
            {
                var result = dict.Query(grapheme);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public string[] UnpackHint(string hint, char separator = ' ')
        {
            foreach (var dict in dictionaries)
            {
                var result = dict.UnpackHint(hint, separator);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
