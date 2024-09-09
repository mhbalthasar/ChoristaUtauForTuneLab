using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Russian
{

    [Phonemeizer("[OU]Russian CVC")]
    public class RussianCVCPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public RussianCVCPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.RussianCVCPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
