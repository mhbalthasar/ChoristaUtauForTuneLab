using System;
using System.Collections.Generic;
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
            {"zu","づ"},
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
            if(voiceBank.DefaultLyric== "あ" && voiceBank.FindSymbol("a",60)==null) return true;
            return false;
        }
    }
}
