using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Spanish
{

    [Phonemeizer("[OU]Spanish Syllable")]
    public class SpanishSYLPhonemizer : BaseAdapterPhonemizer
    {
        public SpanishSYLPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.SpanishSyllableBasedPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
