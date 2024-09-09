using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.English
{

    [Phonemeizer("[OU]English VCCV")]
    public class BrazilianPortugueseCVCPhonemizer : BaseAdapterPhonemizer
    {
        public BrazilianPortugueseCVCPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.EnglishVCCVPhonemizer);

        public override string PhonemizerDictFileName => "envccv.yaml";
    }
}
