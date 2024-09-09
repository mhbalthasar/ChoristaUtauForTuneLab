using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Korean
{

    [Phonemeizer("[OU]Korean CBNN")]
    public class KoreanCBNNPhonemizer : BaseAdapterPhonemizer
    {
        public KoreanCBNNPhonemizer(VoiceBank vb) : base(vb)
        {
            if(this.currentAdapter!=null)this.currentAdapter.bNeedPreProcess = true;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.KoreanCBNNPhonemizer);

        public override string PhonemizerDictFileName => "";
    }
}
