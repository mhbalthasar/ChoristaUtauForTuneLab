using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Vietnamese
{

    [Phonemeizer("[OU]Vietnamese VCV")]
    public class VietnameseVCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public VietnameseVCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.VietnameseVCVPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
