using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ude;
using UtauSharpApi.UNote;
using UtauSharpApi.Utils;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer.Presamp
{
    internal class Presamp_Static
    {
        public static string ParseVersion(string[] lines)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { return l.Trim(); }
            }
            return "";
        }
        public static string ParseSU(string[] lines)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { return l.Trim(); }
            }
            return "";
        }
        public static int ParseLocale(string[] lines)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { if (int.TryParse(l.Trim(), out int rt)) { return rt; }; break; }
            }
            return 0;
        }
        public static List<Presamp_VowelItem> ParseVowel(string[] lines)
        {
            List<Presamp_VowelItem> ret = new List<Presamp_VowelItem>();
            foreach (var l in lines)
            {
                Presamp_VowelItem? iv = Presamp_VowelItem.Parse(l);
                if (iv == null) continue;
                ret.Add(iv);
            }
            return ret;
        }
        public static List<Presamp_ConsonantItem> ParseConsonant(string[] lines, bool defCFlag = false, bool defVCLength = false)
        {
            List<Presamp_ConsonantItem> ret = new List<Presamp_ConsonantItem>();
            foreach (var l in lines)
            {
                Presamp_ConsonantItem? ic = Presamp_ConsonantItem.Parse(l,defCFlag,defVCLength);
                if (ic == null) continue;
                ret.Add(ic);
            }
            return ret;
        }
        public static List<string> ParsePriority(string[] lines)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { return new List<string>(l.Split(',').Select(p => p.Trim())); }
            }
            return new List<string>();
        }
        public static List<Presamp_ReplaceItem> ParseReplace(string[] lines)
        {
            List<Presamp_ReplaceItem> ret = new List<Presamp_ReplaceItem>();
            foreach (var l in lines)
            {
                Presamp_ReplaceItem? ir = Presamp_ReplaceItem.Parse(l);
                if (ir == null) continue;
                ret.Add(ir);
            }
            return ret;
        }
        public static List<Presamp_AliasItem> ParseAlias(string[] lines)
        {
            List<Presamp_AliasItem> ret = new List<Presamp_AliasItem>();
            foreach (var l in lines)
            {
                Presamp_AliasItem? ir = Presamp_AliasItem.Parse(l);
                if (ir == null) continue;
                ret.Add(ir);
            }
            return ret;
        }
        public static List<string> ParsePrefix(string[] lines)
        {
            return new List<string>(lines.Where(l=>l.Trim().Length>0).Select(p=>p.Trim()));
        }
        public static List<string> ParseLines(string[] lines)
        {
            return new List<string>(lines.Where(l => l.Trim().Length > 0).Select(p => p.Trim()));
        }
        public static int ParseInt(string[] lines,int defaultValue=0)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { if (int.TryParse(l.Trim(), out int rt)) { return rt; }; break; }
            }
            return defaultValue;
        }
        public static bool ParseBool(string[] lines, bool defaultValue = false)
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { if (int.TryParse(l.Trim(), out int rt)) { return rt==0?false:true; }; break; }
            }
            return defaultValue;
        }
        public static string ParseString(string[] lines,string defaultValue="")
        {
            foreach (var l in lines)
            {
                if (l.Trim().Length > 0) { return l.Trim(); }
            }
            return defaultValue;
        }
    }

    [ProtoContract]
    public class Presamp_VowelItem
    {
        [ProtoMember(1)]
        public string Vowel { get; private set; } = "";
        [ProtoMember(2)]
        public string Alias { get; private set; } = "";
        [ProtoMember(3)]
        public List<string> ConsonantVowels { get; private set; } = new List<string>();
        [ProtoMember(4)]
        public int Volume { get; private set; } = 100;
        internal static Presamp_VowelItem? Parse(string line)
        {
            string[] SPL = line.Split('=');
            if (SPL.Count() < 3) return null;
            Presamp_VowelItem ret = new Presamp_VowelItem();
            ret.Vowel = SPL[0].Trim();
            ret.Alias = SPL[1].Trim();
            if (SPL.Count() > 3 && int.TryParse(SPL[3].Trim(), out int vol)) { ret.Volume = vol; } else { ret.Volume = 100; };
            ret.ConsonantVowels = new List<string>();
            ret.ConsonantVowels.AddRange(SPL[2].Split(',').Select(p=>p.Trim()));
            if (ret.Alias.Length == 0) ret.Alias = ret.Vowel;
            if (ret.Vowel.Length == 0) return null;
            if (ret.ConsonantVowels.Count == 0) return null;
            return ret;
        }
    }

    [ProtoContract]
    public class Presamp_ConsonantItem
    {
        [ProtoMember(1)]
        public string Consonant { get; private set; } = "";
        [ProtoMember(2)]
        public List<string> ConsonantVowels { get; private set; } = new List<string>();
        [ProtoMember(3)]
        public bool DontCrossFade { get; private set; } = false;//CROSSFADE FLAG，DEFAULT:0,0:CROSSFADE BETWEEN CV & VC,1:DONT CROSSFADE BETWEEN CV-VC
        [ProtoMember(4)]
        public bool SetupVCLengthFromVC { get; private set; } = false; //VCLENGTH,DEFAULT:0,0:FROM CV,1:FROM VC
        internal static Presamp_ConsonantItem? Parse(string line,bool defCFlag=false,bool defVCLength=false)
        {
            string[] SPL = line.Split('=');
            if (SPL.Count() < 2) return null;
            string c = SPL[0].Trim();
            string CVs = SPL[1];
            string CrossFadeFlag = SPL.Count() > 2 ? SPL[2] : "0";
            string VCLENGTH = SPL.Count() > 3 ? SPL[3] : "0";
            Presamp_ConsonantItem ret = new Presamp_ConsonantItem();
            if (c.Length == 0) return null; else ret.Consonant = c;
            ret.ConsonantVowels.AddRange(CVs.Split(',').Select(p=>p.Trim()));
            if (ret.ConsonantVowels.Count == 0) return null;
            ret.DontCrossFade = defCFlag;
            ret.SetupVCLengthFromVC = defVCLength;
            if (int.TryParse(CrossFadeFlag, out int cff)) { if (cff == 1) ret.DontCrossFade = true; else ret.DontCrossFade = false; }
            if (int.TryParse(VCLENGTH, out int vcl)) { if (vcl == 1) ret.SetupVCLengthFromVC = true; else ret.SetupVCLengthFromVC = false; }
            return ret;
        }
    }

    [ProtoContract]
    public class Presamp_ReplaceItem
    {
        [ProtoMember(1)]
        public string Source { get; private set; } = "";
        [ProtoMember(2)]
        public string Target { get; private set; } = "";
        internal static Presamp_ReplaceItem? Parse(string line)
        {
            string[] SPL = line.Split('=');
            if (SPL.Count() < 2) return null;
            Presamp_ReplaceItem ret = new Presamp_ReplaceItem();
            ret.Source = SPL[0].Trim();
            ret.Target = SPL[1].Trim();
            if (ret.Source.Length == 0) return null;
            if (ret.Target.Length == 0) return null;
            return ret;
        }
    }
    [ProtoContract]
    public class Presamp_AliasItem
    {
        [ProtoMember(1)]
        public string Source { get; private set; } = "";
        [ProtoMember(2)]
        public string Target { get; private set; } = "";
        internal static Presamp_AliasItem? Parse(string line)
        {
            string[] SPL = line.Split('=');
            if (SPL.Count() < 2) return null;
            Presamp_AliasItem ret = new Presamp_AliasItem();
            ret.Source = SPL[0].Trim();
            ret.Target = "";
            for (int i=1;i<SPL.Count();i++)
            {
                ret.Target += "="+SPL[i];
            }
            if (ret.Target.Length > 0) ret.Target = ret.Target.Substring(1).Trim();
            if (ret.Source.Length == 0) return null;
            if (ret.Target.Length == 0) return null;
            return ret;
        }
    }

    [ProtoContract]
    public class Presamp
    {
        [ProtoMember(1)]
        public string Version { get; private set; } = "";
        [ProtoMember(2)]
        public int Locale { get; private set; } = 0;
        [ProtoMember(3)]
        public List<Presamp_VowelItem> Vowels { get; private set; } = new List<Presamp_VowelItem>();
        [ProtoMember(4)]
        public List<Presamp_ConsonantItem> Consonants { get; private set; } = new List<Presamp_ConsonantItem>();
        [ProtoMember(5)]
        public List<string> Priority { get; private set; } = new List<string>();
        [ProtoMember(6)]
        public List<Presamp_ReplaceItem> Replace { get; private set; } = new List<Presamp_ReplaceItem>();
        [ProtoMember(7)]
        public List<Presamp_AliasItem> Alias { get; private set; } = new List<Presamp_AliasItem>();
        [ProtoMember(8)]
        public List<string> Prefix { get; private set; } = new List<string>();
        [ProtoMember(9)]
        public string SU { get; private set; } = "";
        [ProtoMember(10)]
        public List<string> Num { get; private set; } = new List<string>();
        [ProtoMember(11)]
        public List<string> Append { get; private set; } = new List<string>();
        [ProtoMember(12)]
        public List<string> Pitch { get; private set; } = new List<string>();
        [ProtoMember(13)]
        public List<string> Alias_Priority { get; private set; } = new List<string>();
        [ProtoMember(14)]
        public List<string> Alias_Priority_DifAppend { get; private set; } = new List<string>();
        [ProtoMember(15)]
        public List<string> Alias_Priority_DifPitch { get; private set; } = new List<string>();
        [ProtoMember(16)]
        public bool Split { get; private set; } = false;
        [ProtoMember(17)]
        public bool MustVC { get; private set; } = false;
        [ProtoMember(18)]
        public string EndType { get; private set; } = "%v% R";
        [ProtoMember(19)]
        public int EndFlag { get; private set; } = 1;
        [ProtoMember(20)]
        public string VCPad { get; private set; } = " ";
        [ProtoMember(21)]
        public string EndType1 { get; private set; } = "";
        [ProtoMember(22)]
        public string EndType2 { get; private set; } = "";

        public List<string> GetAliasFromat(string Type)
        {
            switch(Type.ToUpper())
            {
                case "VCV":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%v%%VCVPAD%%CV%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "BEGINING_CV":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "-%VCVPAD%%CV%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "CROSS_CV":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "*%VCVPAD%%CV%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "VC":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%v%%vcpad%%c%, %c%%vcpad%%c%" : al.Target).Split(",").Select(p=>p.Trim()));
                    }
                case "CV":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%CV%, %c%%V%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "C":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%c%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "V":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%v%" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "LONG_V":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>((al == null ? "%V%-" : al.Target).Split(",").Select(p => p.Trim()));
                    }
                case "VCPAD":
                    {
                        if (VCPad.Length != 0) return new List<string>([VCPad]);
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>([al == null ? " " : al.Target]);
                    }
                case "VCVPAD":
                    {
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>([al == null ? " " : al.Target]);
                    }
                case "ENDING1":
                    {
                        if (EndType1.Length != 0) return new List<string>([EndType1]);
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>([al == null ? "%v%%VCPAD%R" : al.Target]);
                    }
                case "ENDING2":
                    {
                        if (EndType1.Length != 0) return new List<string>([EndType1]);
                        var al = Alias.Find(p => p.Source == Type.ToUpper());
                        return new List<string>([al == null ? "-" : al.Target]);
                    }
                case "ENDING":
                    {
                        return new List<string>([EndType.Length == 0 ? "%v% R" : EndType]);
                    }
                default:
                    return new List<string>([""]);
            }
        }
        private static string DetectFileEncoding(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var detector = new CharsetDetector();
                detector.Feed(fileStream);
                detector.DataEnd();

                if (detector.Charset != null)
                {
                    if (detector.Charset == "KOI8-R") return "GBK";
                    return detector.Charset;
                }
                else
                {
                    return "Shift-JIS";
                }
            }
        }
        public static Presamp? ParsePresamp(string file)
        {
            if (!File.Exists(file)) return null;
            string dir = Path.GetDirectoryName(file);
            string filenoext = Path.GetFileNameWithoutExtension(file);
            string cacheFile = Path.Combine(dir, filenoext + ".protobuf");
            if(File.Exists(cacheFile))
            {
                using (var cacheData = File.OpenRead(cacheFile))
                {
                    return ProtoBuf.Serializer.Deserialize<Presamp>(cacheData);
                }
            }
            Dictionary<string, string[]> iniData = new Dictionary<string, string[]>();
            {
                string EncodName = DetectFileEncoding(file);
                string[] fileLines = File.ReadAllLines(file, EncodingUtils.GetEncoding(EncodName));
                bool inSection = false;
                int SectionStart = -1;
                int SectionEnd = -1;
                string SectionName = "";
                for (int i = 0; i < fileLines.Length; i++)
                {
                    string curLine = fileLines[i].Trim();
                    if (curLine.StartsWith("[") && curLine.EndsWith("]"))
                    {
                        if (inSection)
                        {
                            SectionEnd = i - 1;
                            if (SectionStart > 0 && SectionEnd >= SectionStart)
                            {
                                string[] lines = new string[SectionEnd - SectionStart + 1];
                                Array.Copy(fileLines, SectionStart, lines, 0, SectionEnd - SectionStart + 1);
                                iniData.Add(SectionName, lines);
                            }
                            inSection = false;
                        }
                        if (!inSection)
                        {
                            SectionStart = i + 1;
                            SectionName = curLine.Substring(1, curLine.Length - 2);
                            inSection = true;
                        }
                    }
                }
                if (inSection)
                {
                    SectionEnd = fileLines.Length - 1;
                    if (SectionStart > 0 && SectionEnd >= SectionStart)
                    {
                        string[] lines = new string[SectionEnd - SectionStart + 1];
                        Array.Copy(fileLines, SectionStart, lines, 0, SectionEnd - SectionStart + 1);
                        iniData.Add(SectionName, lines);
                    }
                    inSection = false;
                }
            }

            Presamp ret = new Presamp();
            if (iniData.ContainsKey("VERSION")) ret.Version = Presamp_Static.ParseVersion(iniData["VERSION"]);
            if (iniData.ContainsKey("LOCALE")) ret.Locale = Presamp_Static.ParseLocale(iniData["LOCALE"]);
            if (iniData.ContainsKey("VOWEL")) ret.Vowels = Presamp_Static.ParseVowel(iniData["VOWEL"]);
            bool defCFlag = false;
            bool defVCLength = false;
            if (iniData.ContainsKey("CFLAGS")) defCFlag= Presamp_Static.ParseBool(iniData["CFLAGS"], false);
            if (iniData.ContainsKey("VCLENGTH")) defVCLength = Presamp_Static.ParseBool(iniData["VCLENGTH"], false);
            if (iniData.ContainsKey("CONSONANT")) ret.Consonants = Presamp_Static.ParseConsonant(iniData["CONSONANT"],defCFlag,defVCLength);
            if (iniData.ContainsKey("PRIORITY")) ret.Priority = Presamp_Static.ParsePriority(iniData["PRIORITY"]);
            if (iniData.ContainsKey("REPLACE")) ret.Replace = Presamp_Static.ParseReplace(iniData["REPLACE"]);
            if (iniData.ContainsKey("ALIAS")) ret.Alias = Presamp_Static.ParseAlias(iniData["ALIAS"]);
            if (iniData.ContainsKey("PREFIX")) ret.Prefix = Presamp_Static.ParsePrefix(iniData["PREFIX"]);
            if (iniData.ContainsKey("SU")) ret.SU = Presamp_Static.ParseSU(iniData["SU"]);
            if (iniData.ContainsKey("NUM")) ret.Num = Presamp_Static.ParseLines(iniData["NUM"]);
            if (iniData.ContainsKey("APPEND")) ret.Append = Presamp_Static.ParseLines(iniData["APPEND"]);
            if (iniData.ContainsKey("PITCH")) ret.Pitch = Presamp_Static.ParseLines(iniData["PITCH"]);
            if (iniData.ContainsKey("ALIAS_PRIORITY")) ret.Alias_Priority = Presamp_Static.ParseLines(iniData["ALIAS_PRIORITY"]);
            if (iniData.ContainsKey("ALIAS_PRIORITY_DIFAPPEND")) ret.Alias_Priority_DifAppend = Presamp_Static.ParseLines(iniData["ALIAS_PRIORITY_DIFAPPEND"]);
            if (iniData.ContainsKey("ALIAS_PRIORITY_DIFPITCH")) ret.Alias_Priority_DifPitch = Presamp_Static.ParseLines(iniData["ALIAS_PRIORITY_DIFPITCH"]);
            if (iniData.ContainsKey("SPLIT")) ret.Split = Presamp_Static.ParseBool(iniData["SPLIT"],true);
            if (iniData.ContainsKey("MUSTVC")) ret.MustVC = Presamp_Static.ParseBool(iniData["MUSTVC"],false);
            if (iniData.ContainsKey("ENDTYPE")) ret.EndType = Presamp_Static.ParseString(iniData["ENDTYPE"], "%v% R");
            if (iniData.ContainsKey("ENDFLAG")) ret.EndFlag = Presamp_Static.ParseInt(iniData["ENDFLAG"], 1);
            if (iniData.ContainsKey("VCPAD")) ret.VCPad = Presamp_Static.ParseString(iniData["VCPAD"], " ");
            if (iniData.ContainsKey("ENDTYPE1")) ret.EndType1 = Presamp_Static.ParseString(iniData["ENDTYPE2"], "");
            if (iniData.ContainsKey("ENDTYPE2")) ret.EndType2 = Presamp_Static.ParseString(iniData["ENDTYPE1"], "");
            //CFLAGS
            //VCLENGTH
            using (var cacheData = File.Create(cacheFile))
            {
                ProtoBuf.Serializer.Serialize(cacheData, ret);
            }
            return ret;
        }
    }

    public class PresampSpliter(Presamp sMap,VoiceBank vb)
    {
        public class PresampNote
        {
            public PresampNote() { }
            public PresampNote(UPhonemeNote n) { Symbol = n.Symbol;Duration = n.SymbolMSec; }
            public PresampNote(UMidiNote n) { Symbol = n.Lyric; Duration = n.DurationMSec; }
            public string Symbol { get; set; } = "";
            public double Duration { get; set; } = -1;
        }
        private class ReplaceItem
        {
            Presamp sMap;
            public ReplaceItem(string Format,Presamp sMap)
            {
                this.sMap = sMap;
                if(Format.IndexOf("%vcvpad%")>0)
                {
                    RType = RpType.VCV;
                    SplitedPart.AddRange(Format.Split("%vcvpad%"));
                }else if (Format.IndexOf("%VCVPAD%") > 0)
                {
                    RType = RpType.VCV;
                    SplitedPart.AddRange(Format.Split("%VCVPAD%"));
                }
                else if(Format.IndexOf("%vcpad%") > 0)
                {
                    RType = RpType.CVVC;
                    SplitedPart.AddRange(Format.Split("%vcpad%"));
                }
                else if(Format.IndexOf("%VCPAD%") > 0)
                {
                    RType = RpType.CVVC;
                    SplitedPart.AddRange(Format.Split("%VCPAD%"));
                }else
                {
                    RType = RpType.CV;
                    SplitedPart.Add(Format);
                }
            }
            public enum RpType
            {
                CV,
                VCV,
                CVVC
            }
            public RpType RType { get; set; }=RpType.CV;
            public List<string> SplitedPart { get; set; } = new List<string>();
            public string ConnectedString { get {
                    switch (RType)
                    {
                        case RpType.VCV:
                            return string.Join(sMap.GetAliasFromat("VCVPAD")[0], SplitedPart);
                        case RpType.CVVC:
                            return string.Join(sMap.GetAliasFromat("VCPAD")[0], SplitedPart);
                        case RpType.CV:
                        default:
                            return SplitedPart[0];
                    }
            } }
        }
        private class SymbolItem()
        {
            public string v { get; private set; } = "";
            public string V { get; private set; } = "";
            public string c { get; private set; } = "";
            public string CV { get; private set; } = "";
            public bool bNoCrossFade = false;
            public bool bVCLength = false;
            public void Parse(Presamp sMap, PresampNote note)
            {
                string Symbol = note.Symbol;
                {
                    //DoReplace
                    var repSymbol = sMap.Replace.Find(p => p.Source == Symbol);
                    if (repSymbol != null && repSymbol.Target.Length > 0) Symbol = repSymbol.Target;
                }
                {
                    //AliasSymbol
                    var vowel = sMap.Vowels.Find(p => (p.Alias == Symbol || p.ConsonantVowels.Contains(Symbol)));
                    var consonant = sMap.Consonants.Find(p => (p.ConsonantVowels.Contains(Symbol)));
                    v = vowel == null ? "" : vowel.Vowel;
                    V = vowel == null ? "" : vowel.Alias;
                    c = consonant == null ? "" : consonant.Consonant;
                    CV = Symbol;
                    bNoCrossFade = consonant == null ? false : consonant.DontCrossFade;
                    bVCLength = consonant == null ? false : consonant.SetupVCLengthFromVC;
                }
            }
            public string Replace(string fmtString)
            {
                string ret = fmtString;
                ret = ret.Replace("%v%", v);
                ret = ret.Replace("%V%", V);
                ret = ret.Replace("%c%", c);
                ret = ret.Replace("%CV%", CV);
                return ret;
            }
        }
        private SymbolItem GetSymbolItem(PresampNote note)
        {
            var ret = new SymbolItem();
            ret.Parse(sMap, note);
            return ret;
        }

        private List<PresampNote> SplitCVVCEndNote(PresampNote currentNote)
        {
            List<PresampNote> ret = new List<PresampNote>();
            if (sMap.EndFlag == 0) ret.Add(currentNote);
            else if(sMap.EndFlag == 1)
            {
                ret.Add(currentNote);//CV
                var si = GetSymbolItem(currentNote);
                var ri = new ReplaceItem(sMap.GetAliasFromat("ENDING")[0], sMap);
                switch(ri.RType)
                {
                    case ReplaceItem.RpType.CV:
                    case ReplaceItem.RpType.CVVC:
                        for (int i = 0; i < ri.SplitedPart.Count; i++) { ri.SplitedPart[i] = si.Replace(ri.SplitedPart[i]); }
                        ret.Add(new PresampNote() { Symbol = ri.ConnectedString,Duration=120 });
                        break;
                }
            }
            return ret;
        }

        public List<PresampNote> SplitCVVC(PresampNote currentNote, PresampNote? nextNote, int NoteNumber = 60)
        {
            if (nextNote == null || nextNote.Symbol == "R") return SplitCVVCEndNote(currentNote);

            string GetSymbolString(string FormatType, SymbolItem? cn, SymbolItem? nn)
            {
                List<string> Formats = sMap.GetAliasFromat(FormatType);
                int fmtIndex = 0;
                while (fmtIndex < Formats.Count)
                {
                    string Format = Formats[fmtIndex];
                    fmtIndex++;
                    var ri = new ReplaceItem(Format, sMap);
                    bool flag = true;
                    var fp = ri.SplitedPart[0];
                    if (fp.IndexOf("%v%") > -1) flag = flag & cn.v.Length > 0;
                    if (fp.IndexOf("%c%") > -1) flag = flag & cn.v.Length > 0;
                    if (fp.IndexOf("%V%") > -1) flag = flag & cn.V.Length > 0;
                    if (fp.IndexOf("%CV%") > -1) flag = flag & cn.CV.Length > 0;
                    if (flag) ri.SplitedPart[0] = cn.Replace(ri.SplitedPart[0]); else continue;
                    if (ri.SplitedPart.Count > 1)
                    {
                        fp = ri.SplitedPart[1];
                        if (fp.IndexOf("%v%") > -1) flag = flag & nn.v.Length > 0;
                        if (fp.IndexOf("%c%") > -1) flag = flag & nn.v.Length > 0;
                        if (fp.IndexOf("%V%") > -1) flag = flag & nn.V.Length > 0;
                        if (fp.IndexOf("%CV%") > -1) flag = flag & nn.CV.Length > 0;
                        if (flag) ri.SplitedPart[1] = nn.Replace(ri.SplitedPart[1]); else continue;
                    }
                    return ri.ConnectedString;
                }
                return "";
            }
            List<PresampNote> ret = new List<PresampNote>();
            var cn = GetSymbolItem(currentNote);
            var nn = GetSymbolItem(nextNote);

            //CV
            string cvSymbol = GetSymbolString("CV", cn, nn);
            //Oto? cvOto = vb.FindSymbol(cvSymbol, NoteNumber);
            double cvLen = currentNote.Duration;
            //VC
            string vcSymbol = GetSymbolString("VC", cn, nn);
            Oto? vcOto = vb.FindSymbol(vcSymbol, NoteNumber);

            Oto? cvOto = vb.FindSymbol(nn.CV, NoteNumber);
            double vcLen = 120;
            if (cvOto != null)
            {
                vcLen = cvOto.Preutter;
                if (cvOto.Overlap == 0 && vcLen < 120) vcLen = Math.Min(120, vcLen * 2);
                if (cvOto.Overlap < 0) vcLen = (cvOto.Preutter - cvOto.Overlap);
            }
            cvLen-= vcLen;
            //CVVC
            ret.Add(new PresampNote()
            {
                Symbol = cvSymbol,
                Duration = cvLen
            });
            ret.Add(new PresampNote()
            {
                Symbol = vcSymbol,
                Duration = vcLen
            });
            return ret;
        }
    }
}
