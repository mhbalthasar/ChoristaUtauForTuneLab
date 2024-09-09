using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.French
{

    [Phonemeizer("[OU]French CVVC")]
    public class FrenchCVVCPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public FrenchCVVCPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.FrenchCVVCPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
