using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;

namespace ChoristaUtauApi.UPhonemizer
{
    public interface IPhonemizer
    {
        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex);
        public bool ProcessAble();
    }
}
