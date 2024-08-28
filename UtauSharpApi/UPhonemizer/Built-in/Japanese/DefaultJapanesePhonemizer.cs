using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UNote;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer
{
    internal class DefaultP2JapaneseDict
    {
        static readonly List<string> legalChar = ["ー", "クァ", "クィ", "クゥ", "クェ", "クォ", "クヮ", "グァ", "グィ", "グゥ", "グェ", "グォ", "グヮ", "くぁ", "くぃ", "くぅ", "くぇ", "くぉ", "くゎ", "ぐぁ", "ぐぃ", "ぐぅ", "ぐぇ", "ぐぉ", "ぐゎ", "ゔょ", "ゔゅ", "ゔゃ", "ゔぉ", "ゔぇ", "ゔぃ", "ゔぁ", "ゔ", "ん", "を", "ゑ", "ゐ", "わ", "ゎ", "ろ", "れ", "る", "りょ", "りゅ", "りゃ", "りぇ", "り", "ら", "よ", "ょ", "ゆ", "ゅ", "や", "ゃ", "も", "め", "む", "みょ", "みゅ", "みゃ", "みぇ", "み", "ま", "ぽ", "ぼ", "ほ", "ぺ", "べ", "へ", "ぷ", "ぶ", "ふぉ", "ふぇ", "ふぃ", "ふぁ", "ふ", "ぴょ", "ぴゅ", "ぴゃ", "ぴぇ", "ぴ", "びょ", "びゅ", "びゃ", "びぇ", "び", "ひょ", "ひゅ", "ひゃ", "ひぇ", "ひ", "ぱ", "ば", "は", "の", "ね", "ぬ", "にょ", "にゅ", "にゃ", "にぇ", "に", "な", "どぅ", "ど", "とぅ", "と", "でょ", "でゅ", "でゃ", "でぇ", "でぃ", "で", "てょ", "てゅ", "てゃ", "てぃ", "て", "づ", "つぉ", "つぇ", "つぃ", "つぁ", "つ", "っ", "ぢ", "ちょ", "ちゅ", "ちゃ", "ちぇ", "ち", "だ", "た", "ぞ", "そ", "ぜ", "せ", "ずぃ", "ず", "すぃ", "す", "じょ", "じゅ", "じゃ", "じぇ", "じ", "しょ", "しゅ", "しゃ", "しぇ", "しぃ", "し", "ざ", "さ", "ご", "こ", "げ", "け", "ぐ", "く", "ぎょ", "ぎゅ", "ぎゃ", "ぎぇ", "ぎ", "きょ", "きゅ", "きゃ", "きぇ", "き", "が", "か", "お", "ぉ", "え", "ぇ", "うぉ", "うぇ", "うぃ", "う", "ぅ", "いぇ", "い", "ぃ", "あ", "ぁ", "ヴょ", "ヴゅ", "ヴゃ", "ヴぉ", "ヴぇ", "ヴぃ", "ヴぁ", "ヴョ", "ヴュ", "ヴャ", "ヴォ", "ヴェ", "ヴィ", "ヴァ", "ヴ", "ン", "ヲ", "ヱ", "ヰ", "ワ", "ヮ", "ロ", "レ", "ル", "リョ", "リュ", "リャ", "リェ", "リ", "ラ", "ヨ", "ョ", "ユ", "ュ", "ヤ", "ャ", "モ", "メ", "ム", "ミョ", "ミュ", "ミャ", "ミェ", "ミ", "マ", "ポ", "ボ", "ホ", "ペ", "ベ", "ヘ", "プ", "ブ", "フォ", "フェ", "フィ", "ファ", "フ", "ピョ", "ピュ", "ピャ", "ピェ", "ピ", "ビョ", "ビュ", "ビャ", "ビェ", "ビ", "ヒョ", "ヒュ", "ヒャ", "ヒェ", "ヒ", "パ", "バ", "ハ", "ノ", "ネ", "ヌ", "ニョ", "ニュ", "ニャ", "ニェ", "ニ", "ナ", "ドゥ", "ド", "トゥ", "ト", "デョ", "デュ", "デャ", "デェ", "ディ", "デ", "テョ", "テュ", "テャ", "ティ", "テ", "ヅ", "ツォ", "ツェ", "ツィ", "ツァ", "ツ", "ッ", "ヂ", "チョ", "チュ", "チャ", "チェ", "チ", "ダ", "タ", "ゾ", "ソ", "ゼ", "セ", "ズィ", "ズ", "スィ", "ス", "ジョ", "ジュ", "ジャ", "ジェ", "ジ", "ショ", "シュ", "シャ", "シェ", "シィ", "シ", "ザ", "サ", "ゴ", "コ", "ゲ", "ケ", "グ", "ク", "ギョ", "ギュ", "ギャ", "ギェ", "ギ", "キョ", "キュ", "キャ", "キェ", "キ", "ガ", "カ", "オ", "ォ", "エ", "ェ", "ウォ", "ウェ", "ウィ", "ウ", "ゥ", "イェ", "イ", "ィ", "ア", "ァ"];
        static readonly Dictionary<string, string> romajiMap = new Dictionary<string, string>()
        {
            {"kwa","クァ"},
            {"kwi","クィ"},
            {"kwu","クゥ"},
            {"kwe","クェ"},
            {"kwo","クォ"},
            {"gwa","グァ"},
            {"gwi","グィ"},
            {"gwu","グゥ"},
            {"gwe","グェ"},
            {"gwo","グォ"},
            {"byo","ゔょ"},
            {"byu","ゔゅ"},
            {"bya","ゔゃ"},
            {"vo","ゔぉ"},
            {"ve","ゔぇ"},
            {"vi","ゔぃ"},
            {"va","ゔぁ"},
            {"vu","ゔ"},
            {"n","ん"},
            {"o","お"},
            {"e","え"},
            {"i","い"},
            {"wa","わ"},
            {"ro","ろ"},
            {"re","れ"},
            {"ru","る"},
            {"ryo","りょ"},
            {"ryu","りゅ"},
            {"rya","りゃ"},
            {"rye","りぇ"},
            {"ri","り"},
            {"ra","ら"},
            {"yo","よ"},
            {"yu","ゆ"},
            {"ya","や"},
            {"mo","も"},
            {"me","め"},
            {"mu","む"},
            {"myo","みょ"},
            {"myu","みゅ"},
            {"mya","みゃ"},
            {"mye","みぇ"},
            {"mi","み"},
            {"ma","ま"},
            {"po","ぽ"},
            {"bo","ぼ"},
            {"ho","ほ"},
            {"pe","ぺ"},
            {"be","べ"},
            {"he","へ"},
            {"pu","ぷ"},
            {"bu","ぶ"},
            {"fo","ふぉ"},
            {"fe","ふぇ"},
            {"fi","ふぃ"},
            {"fa","ふぁ"},
            {"fu","ふ"},
            {"pyo","ぴょ"},
            {"pyu","ぴゅ"},
            {"pya","ぴゃ"},
            {"pye","ぴぇ"},
            {"pi","ぴ"},
            {"bye","びぇ"},
            {"bi","び"},
            {"hyo","ひょ"},
            {"hyu","ひゅ"},
            {"hya","ひゃ"},
            {"hye","ひぇ"},
            {"hi","ひ"},
            {"pa","ぱ"},
            {"ba","ば"},
            {"ha","は"},
            {"no","の"},
            {"ne","ね"},
            {"nu","ぬ"},
            {"nyo","にょ"},
            {"nyu","にゅ"},
            {"nya","にゃ"},
            {"nye","にぇ"},
            {"ni","に"},
            {"na","な"},
            {"du","どぅ"},
            {"do","ど"},
            {"tu","とぅ"},
            {"to","と"},
            {"dyo","でょ"},
            {"dyu","でゅ"},
            {"dya","でゃ"},
            {"dye","でぇ"},
            {"di","でぃ"},
            {"de","で"},
            {"tyo","てょ"},
            {"tyu","てゅ"},
            {"tya","てゃ"},
            {"ti","てぃ"},
            {"te","て"},
            {"zu","ず"},
            {"tso","つぉ"},
            {"tse","つぇ"},
            {"tsi","つぃ"},
            {"tsa","つぁ"},
            {"tsu","つ"},
            {"cl","っ"},
            {"ji","ぢ"},
            {"cho","ちょ"},
            {"chu","ちゅ"},
            {"cha","ちゃ"},
            {"che","ちぇ"},
            {"chi","ち"},
            {"da","だ"},
            {"ta","た"},
            {"zo","ぞ"},
            {"so","そ"},
            {"ze","ぜ"},
            {"se","せ"},
            {"zi","ずぃ"},
            {"si","すぃ"},
            {"su","す"},
            {"jo","じょ"},
            {"ju","じゅ"},
            {"ja","じゃ"},
            {"je","じぇ"},
            {"sho","しょ"},
            {"shu","しゅ"},
            {"sha","しゃ"},
            {"she","しぇ"},
            {"shi","し"},
            {"za","ざ"},
            {"sa","さ"},
            {"go","ご"},
            {"ko","こ"},
            {"ge","げ"},
            {"ke","け"},
            {"gu","ぐ"},
            {"ku","く"},
            {"gyo","ぎょ"},
            {"gyu","ぎゅ"},
            {"gya","ぎゃ"},
            {"gye","ぎぇ"},
            {"gi","ぎ"},
            {"kyo","きょ"},
            {"kyu","きゅ"},
            {"kya","きゃ"},
            {"kye","きぇ"},
            {"ki","き"},
            {"ga","が"},
            {"ka","か"},
            {"wo","うぉ"},
            {"we","うぇ"},
            {"wi","うぃ"},
            {"u","う"},
            {"ye","いぇ"},
            {"a","あ"},
            {"-","ー"},
        };
        static readonly Dictionary<string, string> invertedRomajiMap = romajiMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        public static string TransformLyric(string lyric)
        {
            if (legalChar.Contains(lyric)) return lyric;
            if (romajiMap.ContainsKey(lyric.ToLower())) return romajiMap[lyric.ToLower()];
            return "あ";
        }

        public static string GetVowel(string Lyric)
        {
            if (romajiMap.ContainsKey(Lyric)) { return TransformLyric(Lyric.Substring(Lyric.Length - 1)); }
            if (invertedRomajiMap.ContainsKey(Lyric)) { string romaji = invertedRomajiMap[Lyric]; return TransformLyric(romaji.Substring(romaji.Length - 1)); }
            return Lyric;
        }
    }
    public class DefaultJapanesePhonemizer:IPhonemizer
    {
        public List<string> Process(UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<string>() { DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex].Lyric) };
            }
            return new List<string>();
        }

        public List<UPhonemeNote> ProcessEx(VoiceBank voiceBank, UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<UPhonemeNote>() {
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex].Lyric)){
                        SymbolMSec=-1
                    } };
            }
            return new List<UPhonemeNote>();
        }
        public bool ProcessAble(VoiceBank voiceBank)
        {
            if(voiceBank.DefaultLyric== "あ" && voiceBank.FindSymbol("a",60)==null && voiceBank.FindSymbol("u あ", 60)==null) return true;
            return false;
        }
    }
    public class DefaultVCVJapanesePhonemizer : IPhonemizer
    {
        private static readonly List<string> VowelA = new List<string>() { "あ", "か", "が", "さ", "ざ", "た", "だ", "な", "は", "ば", "ぱ", "ま", "や", "ら", "わ", "うぁ", "きゃ", "ぎゃ", "しゃ", "じゃ", "ちゃ", "つぁ", "てゃ", "ぢゃ", "でゃ", "にゃ", "ひゃ", "ぴゃ", "びゃ", "ふぁ", "みゃ", "りゃ", "ゔぁ", "ア", "カ", "ガ", "サ", "ザ", "タ", "ダ", "ナ", "ハ", "バ", "パ", "マ", "ヤ", "ラ", "ワ", "ウァ", "キャ", "ギャ", "シャ", "ジャ", "チャ", "ツァ", "テャ", "ヂャ", "デャ", "ニャ", "ヒャ", "ピャ", "ビャ", "ファ", "ミャ", "リャ", "ヴァ", "ウぁ", "キゃ", "ギゃ", "シゃ", "ジゃ", "チゃ", "ツぁ", "テゃ", "ヂゃ", "デゃ", "ニゃ", "ヒゃ", "ピゃ", "ビゃ", "フぁ", "ミゃ", "リゃ", "ヴぁ", "くぁ", "ぐぁ", "クぁ", "グぁ", "クァ", "グァ", "づぁ", "ヅぁ", "ヅァ", "ふゃ", "フゃ", "フャ" };
        private static readonly List<string> VowelI = new List<string>() { "い","き","ぎ","し","じ","ち","ぢ","に","ひ","び","ぴ","み","いぃ","り","ゐ","うぃ","すぃ","ずぃ","つぃ","てぃ","でぃ","ふぃ","ゔぃ","イ","キ","ギ","シ","ジ","チ","ヂ","ニ","ヒ","ビ","ピ","ミ","イィ","リ","ヰ","ウィ","スィ","ズィ","ツィ","ティ","ディ","フィ","ヴィ","イぃ","ウぃ","ツぃ","テぃ","デぃ","フぃ","ヴぃ","くぃ","ぐぃ","クぃ","グぃ","クィ","グィ","づぃ","ヅぃ","ヅィ"};
        private static readonly List<string> VowelU = new List<string>() {"う","く","ぐ","す","ず","つ","づ","ぬ","ふ","ぶ","ぷ","む","ゆ","る","うぅ","きゅ","ぎゅ","しゅ","じゅ","ちゅ","てゅ","ぢゅ","でゅ","にゅ","ひゅ","ぴゅ","びゅ","ふゅ","みゅ","りゅ","ゔ","ウ","ク","グ","ス","ズ","ツ","ヅ","ヌ","フ","ブ","プ","ム","ユ","ル","ウゥ","キュ","ギュ","シュ","ジュ","チュ","テュ","ヂュ","デュ","ニュ","ヒュ","ピュ","ビュ","フュ","ミュ","リュ","ヴ","ウぅ","キゅ","ギゅ","シゅ","ジゅ","チゅ","テゅ","ヂゅ","デゅ","ニゅ","ヒゅ","ピゅ","ビゅ","フゅ","ミゅ","リゅ","とぅ","どぅ","トぅ","ドぅ","トゥ","ドゥ"};
        private static readonly List<string> VowelE = new List<string>() {"え","け","げ","せ","ぜ","て","で","ね","へ","べ","ぺ","め","いぇ","れ","ゑ","きぇ","ぎぇ","しぇ","じぇ","ちぇ","つぇ","てぇ","ぢぇ","でぇ","にぇ","ひぇ","ぴぇ","びぇ","ふぇ","みぇ","りぇ","ゔぇ","エ","ケ","ゲ","セ","ゼ","テ","デ","ネ","ヘ","ベ","ペ","メ","イェ","レ","ヱ","キェ","ギェ","シェ","ジェ","チェ","ツェ","テェ","ヂェ","デェ","ニェ","ヒェ","ピェ","ビェ","フェ","ミェ","リェ","ヴェ","キぇ","ギぇ","シぇ","ジぇ","チぇ","ツぇ","テぇ","ヂぇ","デぇ","ニぇ","ヒぇ","ピぇ","ビぇ","フぇ","ミぇ","リぇ","ヴぇ","イぇ","うぇ","ウェ","ウぇ","くぇ","ぐぇ","クぇ","グぇ","クェ","グェ","づぇ","ヅぇ","ヅェ"};
        private static readonly List<string> VowelO = new List<string>() {"お","こ","ご","そ","ぞ","と","ど","の","ほ","ぼ","ぽ","も","よ","ろ","を","うぉ","きょ","ぎょ","しょ","じょ","ちょ","ぢょ","てょ","でょ","にょ","ひょ","ぴょ","びょ","ふょ","みょ","りょ","ゔぉ","オ","コ","ゴ","ソ","ゾ","ト","ド","ノ","ホ","ボ","ポ","モ","ヨ","ロ","ヲ","ウォ","キョ","ギョ","ショ","ジョ","チョ","ヂョ","テョ","デョ","ニョ","ヒョ","ピョ","ビョ","フョ","ミョ","リョ","ヴォ","ウぉ","キょ","ギょ","シょ","ジょ","チょ","ヂょ","テょ","デょ","ニょ","ヒょ","ピょ","ビょ","フょ","ミょ","リょ","ヴぉ","くぉ","ぐぉ","クぉ","グぉ","クォ","グォ","ふぉ","フぉ","フォ","つぉ","ツぉ","ツォ","づぉ","ヅぉ","ヅォ"};
        private static readonly List<string> VowelN = new List<string>() {"ん","ン"};

        private static string findVowel(string charSymbol)
        {
            if (charSymbol == "R") return "";
            if (VowelA.Contains(charSymbol)) return "a";
            if (VowelI.Contains(charSymbol)) return "i";
            if (VowelU.Contains(charSymbol)) return "u";
            if (VowelE.Contains(charSymbol)) return "e";
            if (VowelO.Contains(charSymbol)) return "o";
            if (VowelN.Contains(charSymbol)) return "n";
            return "-";
        }
        public List<string> Process(UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                string Symbol = DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex].Lyric);
                string PrevSymbol = (NoteIndex==0 || (MidiPart.Notes[NoteIndex].StartMSec - (MidiPart.Notes[NoteIndex - 1].StartMSec+ MidiPart.Notes[NoteIndex - 1].DurationMSec))>1) ?"R" : DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex - 1].Lyric);
                return new List<string>() {
                    findVowel(PrevSymbol)==""?Symbol:findVowel(PrevSymbol)+" "+Symbol
                };
            }
            return new List<string>();
        }

        public List<UPhonemeNote> ProcessEx(VoiceBank voiceBank, UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                string Symbol = DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex].Lyric);
                string PrevSymbol = (NoteIndex == 0 || (MidiPart.Notes[NoteIndex].StartMSec - (MidiPart.Notes[NoteIndex - 1].StartMSec + MidiPart.Notes[NoteIndex - 1].DurationMSec)) > 1) ? "R" : DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex - 1].Lyric);
                string nSymbol = findVowel(PrevSymbol) + " " + Symbol;
                if (nSymbol.Trim() == Symbol) nSymbol = Symbol;
                
                if (voiceBank.FindSymbol(nSymbol, 60) == null)
                {
                    /*if (nSymbol[0] == 'a' || nSymbol[0] == 'i' || nSymbol[0] == 'u' || nSymbol[0] == 'e' || nSymbol[0] == 'o')
                    {
                        nSymbol= "-" + nSymbol.Substring(1);
                        if (voiceBank.FindSymbol(nSymbol, 60) == null)
                        {
                            nSymbol = Symbol;
                        }
                    }else*/
                        nSymbol = Symbol;
                }
                return new List<UPhonemeNote>() {
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],nSymbol){
                        SymbolMSec=-1
                    } };
            }
            return new List<UPhonemeNote>();
        }
        public bool ProcessAble(VoiceBank voiceBank)
        {
            if (voiceBank.DefaultLyric == "あ" && voiceBank.FindSymbol("u あ", 60) != null)
            {
                return true;
            }
            return false;
        }
    }
}
