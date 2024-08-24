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
    public class DefaultPhonemizer : IPhonemizer
    {
        public List<string> Process(UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex>=0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<string>() { MidiPart.Notes[NoteIndex].Lyric };
            }
            return new List<string>();
        }

        public List<UPhonemeNote> ProcessEx(VoiceBank voiceBank, UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<UPhonemeNote>() {
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],MidiPart.Notes[NoteIndex].Lyric){
                        SymbolMSec=-1
                    } };
            }
            return new List<UPhonemeNote>();
        }
    }
}
