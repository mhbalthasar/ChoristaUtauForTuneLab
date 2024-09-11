using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Japanese
{

    [Phonemeizer("[OU]Japanese (Presamp)")]
    public class JapanesePresampPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public JapanesePresampPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.JapaneseCVVCPhonemizer);

        public override string PhonemizerDictFileName => "presamp.ini";

        protected override bool IsProcessAble(bool defaultValue = false)
        {
            if (voiceBank.FindSymbol("あ", 60) == null &&
                voiceBank.FindSymbol("* あ", 60) == null &&
                voiceBank.FindSymbol("- あ", 60) == null
                )
                return defaultValue;
            return base.IsProcessAble(defaultValue);
        }
    }
}
