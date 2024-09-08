using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using SyllableG2PApi.Syllabler.impl;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

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
            EnglishVCCVSyllabler syllabler = new EnglishVCCVSyllabler(new Func<string, bool>((symbol) => { 
                return voiceBank.FindSymbol(symbol, 60) != null; 
            }));
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
            syllabler.WaitForDictionaryLoaded();
            List<List<string>> currentPhones=new List<List<string>>();
            string neighbourNext = "";
            try
            {
                currentPhones = syllabler.SplitSyllable(
                    curNote.Lyric,
                    prevNote == null ? null : prevNote.Lyric,
                    nextNote == null ? null : nextNote.Lyric,
                    out neighbourNext,
                    out string error);
            }
            catch { return ret; }
            List<string> phonemes = currentPhones.SelectMany(p => p).ToList();

            List<UPhonemeNote> revList = new List<UPhonemeNote>();//倒序Phoneme，用于设置时长（VCCV特有）
            var nextOto = neighbourNext==""?null:voiceBank.FindSymbol(neighbourNext, nextNote.NoteNumber);

            var lastPrtLen = 30.0;
            var ccLen = 30.0;//固定CC长度
            if (nextOto != null)//从下一个音符计算最后一个vel
            {
                lastPrtLen = nextOto.Preutter;
                if (nextOto.Overlap == 0 && lastPrtLen < 120) lastPrtLen = Math.Min(30, lastPrtLen * 2);
                if (nextOto.Overlap < 0) lastPrtLen = (nextOto.Preutter - nextOto.Overlap);
                double dur = curNote.DurationMSec;
                var consonantStretchRatio = Math.Pow(2, 1.0 - nextNote.Velocity * 0.01);
                lastPrtLen = Convert.ToInt32(Math.Min(dur / 1.5, Math.Max(30, lastPrtLen * consonantStretchRatio)));
            }
            for(int i= phonemes.Count-1;i>=0;i--)
            {
                var phoneme = phonemes[i];
                revList.Add(new UPhonemeNote(curNote, phoneme,isCCPart(phoneme)?ccLen: lastPrtLen));
                var oto = voiceBank.FindSymbol(phoneme, curNote.NoteNumber);
                if (oto == null) lastPrtLen = 30;
                else lastPrtLen = oto.Consonant;
            }
            var oriLen= revList.Sum(p => p.SymbolMSec);
            if (oriLen > curNote.DurationMSec)
            {//如果计算总长大于音符长，等比例缩放
                var ratio = curNote.DurationMSec / oriLen;
                foreach (var phn in revList) phn.SymbolMSec *= ratio;
            }else
            {
                //拉伸第一音素（CV）
                if(revList.Count>0)revList.Last().SymbolMSec=-1;
            }
            revList.Reverse();

            return revList;
        }
        //辅助函数
        #region
        bool isCCPart(string phoneme)
        {
            var syllabler = loadSyllablerCache[voiceBank];
            var consonants = syllabler.GetConsonants();
            var spr=phoneme.Replace("-", "").Split(' ');
            if (spr.Length == 2)
            {
                var ret = consonants.Contains(spr[0]) && consonants.Contains(spr[1]);
                if (ret)
                { 
                    var p = "p"; 
                }
                return ret;
            }
            return false;
        }
        #endregion
        #endregion
    }
}
