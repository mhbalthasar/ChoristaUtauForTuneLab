using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UNote;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer.Built_in.English
{
    [Phonemeizer("English VCCV")]
    internal class EnglishVCCVPhonemizer
    {
        private VoiceBank voiceBank;
        public EnglishVCCVPhonemizer(VoiceBank vb)
        {
            voiceBank = vb;
        }
        public bool ProcessAble()
        {
            return false;//ENCVVC?
                         //            return (!Able) ? false : loadCache.ContainsKey(voiceBank);
        }

        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            List<UPhonemeNote> ret = new List<UPhonemeNote>() { new UPhonemeNote(MidiPart.Notes[NoteIndex], MidiPart.Notes[NoteIndex].Lyric, -1) };


            return ret;
        }
    }
}
