using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Korean
{

    [Phonemeizer("[OU]Korean via JPN VB")]
    public class KOtoJAPhonemizer : BaseAdapterPhonemizer
    {
        public KOtoJAPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.KOtoJAPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
