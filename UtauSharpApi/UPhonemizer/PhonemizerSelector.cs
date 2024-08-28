using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer
{
    public class PhonemizerSelector
    {
        public static IPhonemizer GuessPhonemizer(VoiceBank vb)
        {
            IPhonemizer curIP;
            if ((curIP = new DefaultJapanesePhonemizer()).ProcessAble(vb)) return curIP;
            if ((curIP = new DefaultVCVJapanesePhonemizer()).ProcessAble(vb)) return curIP;
            return new DefaultPhonemizer();
        }

        public static IPhonemizer BuildPhonemizer(string phonemizer)
        {
            return new DefaultPhonemizer();
        }
    }
}
