using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Japanese
{

    [Phonemeizer("[OU]Japanese VCV Romaji")]
    public class JapaneseVCVPhonemizerRomaji : JapaneseVCVPhonemizerHiragana
    {
        public JapaneseVCVPhonemizerRomaji(VoiceBank vb) : base(vb)
        {
        }
        public override List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            for (int i = Math.Max(0, NoteIndex - 2); i < Math.Min(MidiPart.Notes.Count, NoteIndex + 3); i++)
            {
                MidiPart.Notes[i].Lyric = WanaKanaNet.WanaKana.ToHiragana(MidiPart.Notes[i].Lyric);
            }
            return base.Process(MidiPart, NoteIndex);
        }
    }
}
