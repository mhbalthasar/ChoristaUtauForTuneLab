using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UPhonemizer.OpenUtauAdapter;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin
{

    [Phonemeizer("[OU]English Arpasing")]
    public class ArpasingPhonemizer : BaseAdapterPhonemizer
    {
        public ArpasingPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ArpasingPhonemizer);

        public override string PhonemizerDictFileName => "arpasing.yaml";
    }
}
