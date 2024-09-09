using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Polish
{

    [Phonemeizer("[OU]Polish CVC")]
    public class PolishCVCPhonemizer : BaseAdapterPhonemizer
    {
        public PolishCVCPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.PolishCVCPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
