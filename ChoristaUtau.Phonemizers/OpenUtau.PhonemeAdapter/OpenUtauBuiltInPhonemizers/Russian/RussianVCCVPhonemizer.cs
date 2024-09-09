using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Russian
{

    [Phonemeizer("[OU]Russian VCCV")]
    public class RussianVCCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public RussianVCCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.RussianVCCVPhonemizer);

        public override string PhonemizerDictFileName => "ru_vccv.yaml";

    }
}
