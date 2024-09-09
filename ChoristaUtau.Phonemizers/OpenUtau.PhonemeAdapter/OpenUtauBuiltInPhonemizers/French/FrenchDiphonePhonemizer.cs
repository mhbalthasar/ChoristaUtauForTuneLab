using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.French
{

    [Phonemeizer("[OU]French Diphone")]
    public class FrenchDiphonePhonemizer : BaseAdapterPhonemizer
    {
        public FrenchDiphonePhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.FrenchCMUSphinxPhonemizer);

        public override string PhonemizerDictFileName => "french.yaml";
    }
}
