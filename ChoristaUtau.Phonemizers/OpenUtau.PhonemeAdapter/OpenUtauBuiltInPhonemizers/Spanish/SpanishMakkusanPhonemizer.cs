using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Spanish
{

    [Phonemeizer("[OU]Spanish Makkusan")]
    public class SpanishMakkusanPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public SpanishMakkusanPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.SpanishMakkusanPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
