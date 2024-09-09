using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UPhonemizer.OpenUtauAdapter;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter
{
    public abstract class BaseAdapterPhonemizer : IPhonemizer
    {
        public abstract Type PhonemizerType { get; }
        public abstract string PhonemizerDictFileName { get; }

        //初始化复杂需要缓存的Phonemizer
        #region
        internal static Dictionary<VoiceBank, PhonemizerProcessedAdapter> loadSyllablerCache = new Dictionary<VoiceBank, PhonemizerProcessedAdapter>();
        private VoiceBank voiceBank;
        public BaseAdapterPhonemizer(VoiceBank vb)
        {
            voiceBank = vb;//缓存vb信息
            if (!loadSyllablerCache.ContainsKey(vb)) { Init(); }//尝试初始化
        }
        void Init()
        {
            var obj = Activator.CreateInstance(PhonemizerType);
            if (obj is OpenUtau.Api.Phonemizer)
            {
                OpenUtau.Api.Phonemizer OUPhonemizer = (OpenUtau.Api.Phonemizer)obj;
                OUPhonemizer.InitPhonemizer(voiceBank);
                PhonemizerProcessedAdapter adapter = new PhonemizerProcessedAdapter(OUPhonemizer);
                loadSyllablerCache.Add(voiceBank, adapter);
            }
        }
        #endregion

        /// <summary>
        /// 判断是否自动选择当前音素器
        /// </summary>
        /// <returns></returns>
        #region
        private static Dictionary<VoiceBank, bool> AbleRecord = new Dictionary<VoiceBank, bool>();
        protected virtual bool IsProcessAble(bool defaultValue=false)
        {
            if (AbleRecord.ContainsKey(voiceBank)) return AbleRecord[voiceBank];
            //看本地有没有自定义字典吧，如果有肯定可以匹配
            string customDict = Path.Combine(voiceBank.vbBasePath, PhonemizerDictFileName);
            if (File.Exists(customDict))
            {
                AbleRecord.Add(voiceBank, true);
                return true;
            }

            //读Character.txt试试
            string ouYaml = Path.Combine(voiceBank.vbBasePath, "character.yaml");
            if (!File.Exists(ouYaml)) return defaultValue;
            try
            {
                YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                Dictionary<object, object> ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(voiceBank.vbBasePath, "character.yaml")));
                if (ouMap.TryGetValue("default_phonemizer", out object phonemizerName))
                {
                    if(PhonemizerType.FullName==(string)phonemizerName)
                    {
                        AbleRecord.Add(voiceBank, true);
                        return true;
                    }
                }
                return defaultValue;
            }
            catch { return defaultValue; }
        }
        public bool ProcessAble()
        {
            return IsProcessAble(false);
        }
        #endregion

        /// <summary>
        /// 拆音
        /// </summary>
        /// <param name="MidiPart"></param>
        /// <param name="NoteIndex"></param>
        /// <returns></returns>
        #region
        public virtual List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            var adapter = loadSyllablerCache[voiceBank];
            return adapter.Process(MidiPart, NoteIndex);
        }
        #endregion
    }
}
