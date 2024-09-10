using ChoristaUtauApi.UPhonemizer.OpenUtauAdapter;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UVoiceBank;
using Tomlyn.Model;
using Tomlyn;

namespace MocaloidSyllablePhonemizer.English
{
    [Phonemeizer("Mocaloid English (Alpha)")]
    public class MocaloidEnglishPhonemizer : BaseAdapterPhonemizer
    {
        VoiceBank voiceBank;
        static Dictionary<VoiceBank, bool> AbleCache = new Dictionary<VoiceBank, bool>();
        public MocaloidEnglishPhonemizer(VoiceBank vb) : base(vb)
        {
            voiceBank = vb;
            if (AbleCache.ContainsKey(vb)) return;
            string g2paToml = Path.Combine(voiceBank.vbBasePath, "g2pa_map.toml");
            string mocaloidIni = Path.Combine(voiceBank.vbBasePath, "mocaloid.ini");
            if (File.Exists(g2paToml) && File.Exists(mocaloidIni))
            {

                string dictData = File.ReadAllText(g2paToml);
                TomlTable? model;
                if (!Toml.TryToModel(dictData, out model, out var message)) return;
                string LangType = "";
                if (model.TryGetValue("Lang", out object langObj)) { if (((TomlTable)langObj).TryGetValue("Type", out object oLangType)) { LangType = ((string)oLangType).ToLower(); }; } else return;
                if (LangType == "english")
                {
                    AbleCache.Add(vb, true);
                    return;
                }
            }
            AbleCache.Add(vb, false);
        }

        public override Type PhonemizerType => typeof(MocaloidEnglishBasePhonemizer);

        public override string PhonemizerDictFileName => "";

        protected override bool IsProcessAble(bool defaultValue = false)
        {
            if (AbleCache.ContainsKey(voiceBank) && AbleCache[voiceBank])
            {
                return base.IsProcessAble(defaultValue);
            }
            return defaultValue;
        }
    }
}
