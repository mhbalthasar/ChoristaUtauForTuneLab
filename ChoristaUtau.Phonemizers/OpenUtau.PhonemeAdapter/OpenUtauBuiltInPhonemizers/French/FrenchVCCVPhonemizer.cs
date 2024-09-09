using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.French
{

    [Phonemeizer("[OU]French VCCV")]
    public class FrenchVCCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public FrenchVCCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.FrenchVCCVPhonemizer);

        public override string PhonemizerDictFileName => "fr_vccv.yaml";

    }
}
