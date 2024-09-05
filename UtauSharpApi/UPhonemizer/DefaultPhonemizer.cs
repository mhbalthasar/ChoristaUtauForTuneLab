using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UNote;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer
{
    [Phonemeizer("Whole Word (CV)")]
    public class DefaultPhonemizer : IPhonemizer
    {
        public DefaultPhonemizer() { }
        public DefaultPhonemizer(VoiceBank voiceBank) { }
        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<UPhonemeNote>() {
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],MidiPart.Notes[NoteIndex].Lyric,-1)};
            }
            return new List<UPhonemeNote>();
        }
        public bool ProcessAble()
        {
            return true;
        }
    }
}
