using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter.Builtin.Chinese
{

    [Phonemeizer("[OU]Chinese CVVC")]
    public class ChineseCVVCPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        public ChineseCVVCPhonemizer(VoiceBank vb) : base(vb)
        {
            this.voiceBank = vb;
        }

        public override Type PhonemizerType => typeof(OpenUtau.Plugin.Builtin.ChineseCVVCPhonemizer);

        public override string PhonemizerDictFileName => "";

        protected override bool IsProcessAble(bool defaultValue = false)
        {
            return base.IsProcessAble(defaultValue);
        }
    }
}
