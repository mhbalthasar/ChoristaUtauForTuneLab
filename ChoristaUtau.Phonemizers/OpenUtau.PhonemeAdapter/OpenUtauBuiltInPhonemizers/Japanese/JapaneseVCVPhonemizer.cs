using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Japanese
{

    [Phonemeizer("[OU]Japanese VCV")]
    public class JapaneseVCVPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public JapaneseVCVPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.JapaneseVCVPhonemizer);

        public override string PhonemizerDictFileName => "";

        protected override bool IsProcessAble(bool defaultValue = false)
        {
            return base.IsProcessAble(defaultValue);
        }
    }
}
