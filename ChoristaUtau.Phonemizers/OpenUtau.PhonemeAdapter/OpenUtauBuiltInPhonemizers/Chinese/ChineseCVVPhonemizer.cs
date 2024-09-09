using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Chinese
{

    [Phonemeizer("[OU]Chinese CVV Extend")]
    public class ChineseCVVPhonemizer : BaseAdapterPhonemizer
    {
        public ChineseCVVPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ChineseCVVMonophonePhonemizer);

        public override string PhonemizerDictFileName => "zhcvv.yaml";
    }
}
