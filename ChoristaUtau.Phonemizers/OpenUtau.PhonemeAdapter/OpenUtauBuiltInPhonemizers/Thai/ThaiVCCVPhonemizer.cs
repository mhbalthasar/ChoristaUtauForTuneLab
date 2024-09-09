using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Thai
{

    [Phonemeizer("[OU]Thai VCCV")]
    public class ThaiVCCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public ThaiVCCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ThaiVCCVPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
