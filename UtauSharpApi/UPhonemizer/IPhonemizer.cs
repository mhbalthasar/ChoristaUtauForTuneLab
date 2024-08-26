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
    public interface IPhonemizer
    {
        public List<string> Process(UMidiPart MidiPart, int NoteIndex);
        public List<UPhonemeNote> ProcessEx(VoiceBank voiceBank,UMidiPart MidiPart, int NoteIndex);
        public bool ProcessAble(VoiceBank voiceBank);
    }
}
