using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Vietnamese
{

    [Phonemeizer("[OU]Vietnamese CVVC")]
    public class VietnameseCVVCPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public VietnameseCVVCPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.VietnameseCVVCPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
