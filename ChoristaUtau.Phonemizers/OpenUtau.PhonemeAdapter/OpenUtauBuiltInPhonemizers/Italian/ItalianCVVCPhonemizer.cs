using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Italian
{

    [Phonemeizer("[OU]Italian CVVC")]
    public class ItalianCVVCPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public ItalianCVVCPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ItalianCVVCPhonemizer);

        public override string PhonemizerDictFileName => "";

    }
}
