using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Spanish
{

    [Phonemeizer("[OU]Spanish VCCV")]
    public class SpanishVCCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public SpanishVCCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.SpanishVCCVPhonemizer);

        public override string PhonemizerDictFileName => "es_vccv.yaml";

    }
}
