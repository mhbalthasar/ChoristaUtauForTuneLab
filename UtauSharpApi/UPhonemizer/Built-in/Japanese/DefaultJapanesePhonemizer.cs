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
        static readonly Dictionary<string, string> romajiMap = new Dictionary<string, string>()
        {
            { "N", "ン" },
            { "a", "あ" },
            { "ba", "ば" },
            { "be", "べ" },
            { "bi", "び" },
            { "bo", "ぼ" },
            { "bu", "ぶ" },
            { "bya", "びゃ" },
            { "bye", "びぇ" },
            { "byi", "びぃ" },
            { "byo", "びょ" },
            { "byu", "びゅ" },
            { "cha", "ちゃ" },
            { "che", "ちぇ" },
            { "chi", "ち" },
            { "cho", "ちょ" },
            { "chu", "ちゅ" },
            { "da", "だ" },
            { "de", "で" },
            { "dha", "でゃ" },
            { "dhe", "でぇ" },
            { "dhi", "でぃ" },
            { "dho", "でょ" },
            { "dhu", "でゅ" },
            { "di", "でぃ" },
            { "do", "ど" },
            { "du", "どぅ" },
            { "e", "え" },
            { "fa", "ふぁ" },
            { "fe", "ふぇ" },
            { "fi", "ふぃ" },
            { "fo", "ふぉ" },
            { "fu", "ふ" },
            { "fyu", "ふゅ" },
            { "ga", "が" },
            { "ge", "げ" },
            { "gi", "ぎ" },
            { "go", "ご" },
            { "gu", "ぐ" },
            { "gya", "ぎゃ" },
            { "gye", "ぎぇ" },
            { "gyi", "ぎ" },
            { "gyo", "ぎょ" },
            { "gyu", "ぎゅ" },
            { "ha", "は" },
            { "he", "へ" },
            { "hi", "ひ" },
            { "ho", "ほ" },
            { "hu", "ふ" },
            { "hya", "ひゃ" },
            { "hye", "ひぇ" },
            { "hyi", "ひ" },
            { "hyo", "ひょ" },
            { "hyu", "ひゅ" },
            { "i", "い" },
            { "ja", "じゃ" },
            { "je", "じぇ" },
            { "ji", "じ" },
            { "jo", "じょ" },
            { "ju", "じゅ" },
            { "ka", "か" },
            { "ke", "け" },
            { "ki", "き" },
            { "ko", "こ" },
            { "ku", "く" },
            { "kya", "きゃ" },
            { "kye", "きぇ" },
            { "kyi", "き" },
            { "kyo", "きょ" },
            { "kyu", "きゅ" },
            { "ma", "ま" },
            { "me", "め" },
            { "mi", "み" },
            { "mo", "も" },
            { "mu", "む" },
            { "mya", "みゃ" },
            { "mye", "みぇ" },
            { "myi", "み" },
            { "myo", "みょ" },
            { "myu", "みゅ" },
            { "n", "ん" },
            { "na", "な" },
            { "ne", "ね" },
            { "ni", "に" },
            { "no", "の" },
            { "nu", "ぬ" },
            { "nya", "にゃ" },
            { "nye", "にぇ" },
            { "nyi", "に" },
            { "nyo", "にょ" },
            { "nyu", "にゅ" },
            { "o", "お" },
            { "pa", "ぱ" },
            { "pe", "ぺ" },
            { "pi", "ぴ" },
            { "po", "ぽ" },
            { "pu", "ぷ" },
            { "pya", "ぴゃ" },
            { "pye", "ぴぇ" },
            { "pyi", "ぴ" },
            { "pyo", "ぴょ" },
            { "pyu", "ぴゅ" },
            { "ra", "ら" },
            { "re", "れ" },
            { "ri", "り" },
            { "ro", "ろ" },
            { "ru", "る" },
            { "rya", "りゃ" },
            { "rye", "りぇ" },
            { "ryi", "り" },
            { "ryo", "りょ" },
            { "ryu", "りゅ" },
            { "sa", "さ" },
            { "se", "せ" },
            { "sha", "しゃ" },
            { "she", "しぇ" },
            { "shi", "し" },
            { "sho", "しょ" },
            { "shu", "しゅ" },
            { "si", "すぃ" },
            { "so", "そ" },
            { "su", "す" },
            { "ta", "た" },
            { "te", "て" },
            { "ti", "てぃ" },
            { "to", "と" },
            { "tsa", "つぁ" },
            { "tse", "つぇ" },
            { "tsi", "つぃ" },
            { "tso", "つぉ" },
            { "tsu", "つ" },
            { "tu", "とぅ" },
            { "tya", "てゃ" },
            { "tye", "てぇ" },
            { "tyi", "てぃ" },
            { "tyo", "てょ" },
            { "tyu", "てゅ" },
            { "u", "う" },
            { "va", "ヴぁ" },
            { "ve", "ヴぇ" },
            { "vi", "ヴぃ" },
            { "vo", "ヴぉ" },
            { "vu", "ヴ" },
            { "vyu", "ヴゅ" },
            { "wa", "わ" },
            { "we", "うぇ" },
            { "wi", "うぃ" },
            { "wo", "うぉ" },
            { "wu", "うぅ" },
            { "ya", "や" },
            { "ye", "いぇ" },
            { "yi", "い" },
            { "yo", "よ" },
            { "yu", "ゆ" },
            { "za", "ざ" },
            { "ze", "ぜ" },
            { "zi", "ずぃ" },
            { "zo", "ぞ" },
            { "zu", "ず" },
            { "zya", "じゃ" },
            { "zye", "じぇ" },
            { "zyi", "じ" },
            { "zyo", "じょ" },
            { "zyu", "じゅ" },
            { "を", "お" },
            { "ぢ", "じ" },
            { "づ", "ず" }
        };
        //static readonly Dictionary<string, string> invertedRomajiMap = romajiMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        public static string TransformLyric(string lyric)
        {
            if (romajiMap.ContainsValue(lyric)) return lyric;
            if (romajiMap.ContainsKey(lyric.ToLower())) return romajiMap[lyric.ToLower()];
            return "あ";
        }

    }
    [Phonemeizer("Japanese Romaji")]
    public class DefaultJapanesePhonemizer(VoiceBank voiceBank) :IPhonemizer
    {
        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            if (NoteIndex >= 0 && NoteIndex < MidiPart.Notes.Count)
            {
                return new List<UPhonemeNote>() {
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],DefaultP2JapaneseDict.TransformLyric(MidiPart.Notes[NoteIndex].Lyric),-1)
                };
            }
            return new List<UPhonemeNote>();
        }
        public bool ProcessAble()
        {
            if(voiceBank.DefaultLyric== "あ" && voiceBank.FindSymbol("a",60)==null && voiceBank.FindSymbol("u あ", 60)==null) return true;
            return false;
        }
    }

    [Phonemeizer("Japanese VCV")]
    public class DefaultVCVJapanesePhonemizer(VoiceBank voiceBank) : IPhonemizer
    {
        private static readonly List<string> VowelA = new List<string>() { "あ", "か", "が", "さ", "ざ", "た", "だ", "な", "は", "ば", "ぱ", "ま", "や", "ら", "わ", "うぁ", "きゃ", "ぎゃ", "しゃ", "じゃ", "ちゃ", "つぁ", "てゃ", "ぢゃ", "でゃ", "にゃ", "ひゃ", "ぴゃ", "びゃ", "ふぁ", "みゃ", "りゃ", "ゔぁ", "ア", "カ", "ガ", "サ", "ザ", "タ", "ダ", "ナ", "ハ", "バ", "パ", "マ", "ヤ", "ラ", "ワ", "ウァ", "キャ", "ギャ", "シャ", "ジャ", "チャ", "ツァ", "テャ", "ヂャ", "デャ", "ニャ", "ヒャ", "ピャ", "ビャ", "ファ", "ミャ", "リャ", "ヴァ", "ウぁ", "キゃ", "ギゃ", "シゃ", "ジゃ", "チゃ", "ツぁ", "テゃ", "ヂゃ", "デゃ", "ニゃ", "ヒゃ", "ピゃ", "ビゃ", "フぁ", "ミゃ", "リゃ", "ヴぁ", "くぁ", "ぐぁ", "クぁ", "グぁ", "クァ", "グァ", "づぁ", "ヅぁ", "ヅァ", "ふゃ", "フゃ", "フャ" };
        private static readonly List<string> VowelI = new List<string>() { "い","き","ぎ","し","じ","ち","ぢ","に","ひ","び","ぴ","み","いぃ","り","ゐ","うぃ","すぃ","ずぃ","つぃ","てぃ","でぃ","ふぃ","ゔぃ","イ","キ","ギ","シ","ジ","チ","ヂ","ニ","ヒ","ビ","ピ","ミ","イィ","リ","ヰ","ウィ","スィ","ズィ","ツィ","ティ","ディ","フィ","ヴィ","イぃ","ウぃ","ツぃ","テぃ","デぃ","フぃ","ヴぃ","くぃ","ぐぃ","クぃ","グぃ","クィ","グィ","づぃ","ヅぃ","ヅィ"};
        private static readonly List<string> VowelU = new List<string>() {"う","く","ぐ","す","ず","つ","づ","ぬ","ふ","ぶ","ぷ","む","ゆ","る","うぅ","きゅ","ぎゅ","しゅ","じゅ","ちゅ","てゅ","ぢゅ","でゅ","にゅ","ひゅ","ぴゅ","びゅ","ふゅ","みゅ","りゅ","ゔ","ウ","ク","グ","ス","ズ","ツ","ヅ","ヌ","フ","ブ","プ","ム","ユ","ル","ウゥ","キュ","ギュ","シュ","ジュ","チュ","テュ","ヂュ","デュ","ニュ","ヒュ","ピュ","ビュ","フュ","ミュ","リュ","ヴ","ウぅ","キゅ","ギゅ","シゅ","ジゅ","チゅ","テゅ","ヂゅ","デゅ","ニゅ","ヒゅ","ピゅ","ビゅ","フゅ","ミゅ","リゅ","とぅ","どぅ","トぅ","ドぅ","トゥ","ドゥ"};
        private static readonly List<string> VowelE = new List<string>() {"え","け","げ","せ","ぜ","て","で","ね","へ","べ","ぺ","め","いぇ","れ","ゑ","きぇ","ぎぇ","しぇ","じぇ","ちぇ","つぇ","てぇ","ぢぇ","でぇ","にぇ","ひぇ","ぴぇ","びぇ","ふぇ","みぇ","りぇ","ゔぇ","エ","ケ","ゲ","セ","ゼ","テ","デ","ネ","ヘ","ベ","ペ","メ","イェ","レ","ヱ","キェ","ギェ","シェ","ジェ","チェ","ツェ","テェ","ヂェ","デェ","ニェ","ヒェ","ピェ","ビェ","フェ","ミェ","リェ","ヴェ","キぇ","ギぇ","シぇ","ジぇ","チぇ","ツぇ","テぇ","ヂぇ","デぇ","ニぇ","ヒぇ","ピぇ","ビぇ","フぇ","ミぇ","リぇ","ヴぇ","イぇ","うぇ","ウェ","ウぇ","くぇ","ぐぇ","クぇ","グぇ","クェ","グェ","づぇ","ヅぇ","ヅェ"};
        private static readonly List<string> VowelO = new List<string>() {"お","こ","ご","そ","ぞ","と","ど","の","ほ","ぼ","ぽ","も","よ","ろ","を","うぉ","きょ","ぎょ","しょ","じょ","ちょ","ぢょ","てょ","でょ","にょ","ひょ","ぴょ","びょ","ふょ","みょ","りょ","ゔぉ","オ","コ","ゴ","ソ","ゾ","ト","ド","ノ","ホ","ボ","ポ","モ","ヨ","ロ","ヲ","ウォ","キョ","ギョ","ショ","ジョ","チョ","ヂョ","テョ","デョ","ニョ","ヒョ","ピョ","ビョ","フョ","ミョ","リョ","ヴォ","ウぉ","キょ","ギょ","シょ","ジょ","チょ","ヂょ","テょ","デょ","ニょ","ヒょ","ピょ","ビょ","フょ","ミょ","リょ","ヴぉ","くぉ","ぐぉ","クぉ","グぉ","クォ","グォ","ふぉ","フぉ","フォ","つぉ","ツぉ","ツォ","づぉ","ヅぉ","ヅォ"};
        private static readonly List<string> VowelN = new List<string>() {"ん","ン"};

        private static string findVowel(string charSymbol)
        {
            if (charSymbol == "R") return "-";
            if (VowelA.Contains(charSymbol)) return "a";
            if (VowelI.Contains(charSymbol)) return "i";
            if (VowelU.Contains(charSymbol)) return "u";
            if (VowelE.Contains(charSymbol)) return "e";
            if (VowelO.Contains(charSymbol)) return "o";
            if (VowelN.Contains(charSymbol)) return "n";
            return "-";
        }

        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
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
                    new UPhonemeNote(MidiPart.Notes[NoteIndex],nSymbol,-1)
                };
            }
            return new List<UPhonemeNote>();
        }
        public bool ProcessAble()
        {
            if (voiceBank.DefaultLyric == "あ" && voiceBank.FindSymbol("u あ", 60) != null)
            {
                return true;
            }
            return false;
        }
    }
}
