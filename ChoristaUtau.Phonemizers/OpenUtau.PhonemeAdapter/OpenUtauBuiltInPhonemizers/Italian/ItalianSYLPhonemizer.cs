using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Italian
{

    [Phonemeizer("[OU]Italian Syllable")]
    public class ItalianSYLPhonemizer : BaseAdapterPhonemizer
    {
        public ItalianSYLPhonemizer(VoiceBank vb) : base(vb)
        {
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ItalianSyllableBasedPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
