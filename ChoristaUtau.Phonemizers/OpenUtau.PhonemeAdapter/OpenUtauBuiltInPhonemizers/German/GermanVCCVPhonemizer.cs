using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.German
{

    [Phonemeizer("[OU]German VCCV")]
    public class GermanVCCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public GermanVCCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.GermanVCCVPhonemizer);

        public override string PhonemizerDictFileName => "de_vccv.yaml";

    }
}
