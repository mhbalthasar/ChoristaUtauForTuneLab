using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Vietnamese
{

    [Phonemeizer("[OU]Vietnamese VINA")]
    public class VietnameseVINAPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public VietnameseVINAPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.VietnameseVINAPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
