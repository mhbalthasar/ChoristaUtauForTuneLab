using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.German
{

    [Phonemeizer("[OU]German Diphone")]
    public class GermanDiphonePhonemizer : BaseAdapterPhonemizer
    {
        public GermanDiphonePhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.GermanDiphonePhonemizer);

        public override string PhonemizerDictFileName => "german.yaml";
    }
}
