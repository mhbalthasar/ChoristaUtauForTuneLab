using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using SyllableG2PApi.Syllabler.impl;
using System.Net.Http.Headers;

namespace ChoristaUtau.EnglishVCCV
{

    [Phonemeizer("English VCCV")]
    public class EnglishVCCVPhonemizer : IPhonemizer
    {
        //初始化复杂需要缓存的Phonemizer
        #region
        internal static Dictionary<VoiceBank, EnglishVCCVSyllabler> loadSyllablerCache = new Dictionary<VoiceBank, EnglishVCCVSyllabler>();
        private VoiceBank voiceBank;
        public EnglishVCCVPhonemizer(VoiceBank vb)
        {
            voiceBank = vb;//缓存vb信息
            if (!loadSyllablerCache.ContainsKey(vb)) { Init(); }//尝试初始化
        }
        void Init()
        {
            string customDict = Path.Combine(voiceBank.vbBasePath, "envccv.yaml");//查看是否有字典
            EnglishVCCVSyllabler syllabler = new EnglishVCCVSyllabler(new Func<string, bool>((symbol) => { return voiceBank.FindSymbol(symbol, 60) != null; }));
            if (File.Exists(customDict)) syllabler.SingerPath = voiceBank.vbBasePath;
            syllabler.InitDict(true);//异步加载字典
            loadSyllablerCache.Add(voiceBank, syllabler);//加入缓存
        }
        #endregion

        /// <summary>
        /// 判断是否自动选择当前音素器
        /// </summary>
        /// <returns></returns>
        #region
        public bool ProcessAble()
        {
            //无法主动判断是否是vccv，所以看本地有没有自定义字典吧
            string customDict = Path.Combine(voiceBank.vbBasePath, "envccv.yaml");
            return File.Exists(customDict);
        }
        #endregion

        /// <summary>
        /// 拆音
        /// </summary>
        /// <param name="MidiPart"></param>
        /// <param name="NoteIndex"></param>
        /// <returns></returns>
        #region
        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {   
            UMidiNote? prevNote = NoteIndex > 0 ? MidiPart.Notes[NoteIndex - 1] : null;
            UMidiNote? curNote = MidiPart.Notes[NoteIndex];
            UMidiNote? nextNote = (NoteIndex + 1) < MidiPart.Notes.Count ? MidiPart.Notes[NoteIndex + 1] : null;
            var ret = new List<UPhonemeNote>() { new UPhonemeNote(curNote,curNote.Lyric) };

            var syllabler = loadSyllablerCache[voiceBank];
            List<List<string>> sylPhones;
           // try
            {
                sylPhones = syllabler.SplitSyllable(
                    curNote.Lyric,
                    prevNote == null ? null : prevNote.Lyric,
                    nextNote == null ? null : nextNote.Lyric,
                    out string error);
            }
          //  catch { return ret; }

            return ret;
        }
        //辅助函数
        #region
        #endregion
        #endregion
    }
}
