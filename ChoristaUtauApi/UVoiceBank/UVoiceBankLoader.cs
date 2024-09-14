using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ude;
using ChoristaUtauApi.Utils;
using System.Security.Cryptography;
using System.Linq.Expressions;

namespace ChoristaUtauApi.UVoiceBank
{
    public class UVoiceBankLoader
    {
        public static int OtoSearchDeeply = 3;
        private static string GetFileHash(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        private static string GetMixedHash(List<string> hashes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(string.Join("\r\n", hashes));
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        private static string GetVBHash(string VoiceBankPath, List<string[]> OtoFiles,string Salt="")
        {
            List<string> subHash = new List<string>();
            if (File.Exists(Path.Combine(VoiceBankPath, "prefix.map"))) { subHash.Add(GetFileHash(Path.Combine(VoiceBankPath, "prefix.map"))); }
            if (File.Exists(Path.Combine(VoiceBankPath, "character.txt"))) { subHash.Add(GetFileHash(Path.Combine(VoiceBankPath, "character.txt"))); }
            if (File.Exists(Path.Combine(VoiceBankPath, "character.yaml"))) { subHash.Add(GetFileHash(Path.Combine(VoiceBankPath, "character.yaml"))); }
            foreach (string[] otoPathMap in OtoFiles)
            {
                string otoPath = Path.Combine(otoPathMap);
                string curPath = Path.Combine(VoiceBankPath, otoPath);
                if (File.Exists(curPath))
                {
                    subHash.Add(GetFileHash(curPath));
                }
            }
            subHash.Insert(0,Salt);
            return GetMixedHash(subHash);
        }
        private static List<string[]> SearchOto(string basePath, int deeply = 0)
        {
            if (OtoSearchDeeply > -1 && deeply >= OtoSearchDeeply) return new List<string[]>();

            List<string[]> ret = new List<string[]>();
            if (File.Exists(Path.Combine(basePath, "oto.ini"))) ret.Add(["oto.ini"]);
            foreach (string subdir in Directory.GetDirectories(basePath))
            {
                string curP = Path.GetFileName(subdir);
                var sub = SearchOto(subdir, deeply >= 0 ? deeply + 1 : -1);
                foreach (string[] srr in sub)
                {
                    List<string> newStr = new List<string>();
                    newStr.Add(curP);
                    newStr.AddRange(srr);
                    ret.Add(newStr.ToArray());
                }
            }
            return ret;
        }
        private static string DetectFileEncoding(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    var detector = new CharsetDetector();
                    detector.Feed(fileStream);
                    detector.DataEnd();

                    if (detector.Charset != null)
                    {
                        if (detector.Charset == "KOI8-R") return "GBK";
                        if (detector.Charset == "windows-1252") return "Shift-JIS";//DEFAULT ERROR
                        return detector.Charset;
                    }
                    else
                    {
                        return "Shift-JIS";
                    }
                }
                catch { return "Shift-JIS"; }
            }
        }
        public static VoiceBank LoadVoiceBank(string VoiceBankPath)
        {
            string fmtVersion = "0.0.1";
            List<string[]> otoFiles = SearchOto(VoiceBankPath);
            string hash = GetVBHash(VoiceBankPath, otoFiles,fmtVersion);
            string cache = Path.Combine(VoiceBankPath, "uvoicebank.protobuf");
            if (File.Exists(cache))
            {
                try
                {
                    var vbc = VoiceBank.Deserialize(cache);
                    vbc.vbBasePath = VoiceBankPath;
                    if (vbc.CacheHash == hash)
                        return vbc;
                }
                catch { }
            }

            VoiceBank vb = new VoiceBank();
            vb.CacheHash = hash;


            bool bOUOverlay_Name = false;
            bool bOUOverlay_Prefix = false;


            string ou_detected_encoding = "";
            List<KeyValuePair<PrefixItem, Tuple<int, int>>> ou_prefix_callback = new List<KeyValuePair<PrefixItem, Tuple<int, int>>>();
            //OverlayOU
            if (File.Exists(Path.Combine(VoiceBankPath, "character.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    Dictionary<object, object> ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(VoiceBankPath, "character.yaml")));
                    if (ouMap.TryGetValue("name", out object nameValue))
                    {
                        vb.Name = (string)nameValue;
                        bOUOverlay_Name = true;
                    }

                    if (ouMap.TryGetValue("text_file_encoding", out object encodingValue)) ou_detected_encoding = (string)encodingValue;

                    if (ouMap.TryGetValue("subbanks", out object subPrefix))
                    {
                        Dictionary<string, PrefixItem> Pairs = new Dictionary<string, PrefixItem>();
                        foreach (var obj in (List<object>)subPrefix)
                        {
                            Dictionary<object, object> bankMap = (Dictionary<object, object>)obj;
                            string prefix = "";
                            string suffix = "";
                            int pitch = -1;
                            bool flag = false;
                            if (bankMap.TryGetValue("prefix", out object oPrefix)) { prefix = (string)oPrefix; flag = true; }
                            if (bankMap.TryGetValue("suffix", out object oSuffix)) { suffix = (string)oSuffix; flag = true; }

                            var Pfx = new PrefixItem() { prefix = prefix, suffix = suffix, PitchNumber = pitch };
                            if (flag)
                            {
                                string kPair = prefix.Trim() + suffix.Trim();
                                if (kPair.Trim() == "") kPair = "<No Prefix>";
                                Pairs.Add(kPair, Pfx);
                            }

                            if(bankMap.TryGetValue("tone_ranges",out object oToneRanges))
                            {
                                try
                                {
                                    if (oToneRanges is List<object>)
                                    {
                                        var lToneRange = (List<object>)oToneRanges;
                                        foreach (var t in lToneRange)
                                        {
                                            string[] tr = ((string)t).Trim().Split("-");
                                            Tuple<int, int> vr = new Tuple<int, int>(OctaveUtils.Str2NoteNumber(tr[0].Trim()), OctaveUtils.Str2NoteNumber(tr[1].Trim()));
                                            ou_prefix_callback.Add(new KeyValuePair<PrefixItem, Tuple<int, int>>(
                                                Pfx,vr
                                            ));
                                        }
                                    }
                                }
                                catch {; }
                            }
                        }
                        if (Pairs.Count > 0)
                        {
                            vb.SetPrefixPairs(Pairs);
                            bOUOverlay_Prefix = true;
                        }
                    }
                }
                catch {; }
            }

            //ReadVBName
            if (!bOUOverlay_Name)
            {
                vb.Name = Path.GetFileNameWithoutExtension(VoiceBankPath);
                try{
                    if (File.Exists(Path.Combine(VoiceBankPath, "character.txt")))
                    {
                        string EncodName = ou_detected_encoding.Length > 0 ? ou_detected_encoding : DetectFileEncoding(Path.Combine(VoiceBankPath, "character.txt"));
                        string[] characterText = File.ReadAllLines(Path.Combine(VoiceBankPath, "character.txt"), EncodingUtils.GetEncoding(EncodName));// Path.Combine(VoiceBankPath, "character.txt"));
                        foreach (string line in characterText)
                        {
                            if (line.ToLower().StartsWith("name="))
                            {
                                vb.Name = line.Substring(5);
                                break;
                            }
                        }
                    }
                }
                catch {; }
            }
            if(vb.Name.Trim().Length == 0)
            {
                vb.Name = Path.GetFileNameWithoutExtension(VoiceBankPath);
            }

            //ReadVBPrefix
            if (File.Exists(Path.Combine(VoiceBankPath, "prefix.map")))
            {
                try
                {
                    Dictionary<string, PrefixItem> Pairs = new Dictionary<string, PrefixItem>();
                    string EncodName = ou_detected_encoding.Length > 0 ? ou_detected_encoding : DetectFileEncoding(Path.Combine(VoiceBankPath, "prefix.map"));
                    string[] prefixText = File.ReadAllLines(Path.Combine(VoiceBankPath, "prefix.map"), EncodingUtils.GetEncoding(EncodName));
                    foreach (string line in prefixText)
                    {
                        string[] pfx = line.Split('\t');
                        string noteStr = pfx[0];
                        int nn = OctaveUtils.Str2NoteNumber(noteStr);
                        if (nn < 128 && nn > -1)
                        {
                            string prefixStr = pfx[1];
                            string suffixStr = pfx[2];
                            vb.SetPrefixItem(nn, prefixStr, suffixStr);
                            string kPair = prefixStr.Trim() + suffixStr.Trim();
                            if (kPair.Trim() == "") kPair = "<No Prefix>";
                            if (!Pairs.ContainsKey(kPair))
                            {
                                Pairs.Add(kPair, vb.GetPrefixItem(nn));
                            }
                        }
                    }
                    if (!bOUOverlay_Prefix)
                    {
                        vb.SetPrefixPairs(Pairs);
                    }
                }
                catch {; }
            }else if(ou_prefix_callback.Count>0)
            {
                foreach(var kvp in ou_prefix_callback)
                {
                    try
                    {
                        var Pfx = kvp.Key;
                        var oRange = kvp.Value;
                        for (int i = oRange.Item1; i <= oRange.Item2; i++)
                        {
                            vb.SetPrefixItem(i, Pfx.prefix, Pfx.suffix);
                        }
                    }
                    catch {; }
                }
            }

           

            //LoadOto
            string otofmt (string sin)
            {
                return sin.Trim();
            }
            List<string> otoDirList = new List<string>();
            List<PrefixItem> otoFindedPrefix=new List<PrefixItem>();
            foreach (string[] otoPathMap in otoFiles)
            {
                string otoPath = Path.Combine(otoPathMap);
                string curPath = Path.Combine(VoiceBankPath, otoPath);
                if (File.Exists(curPath))
                {
                    try
                    {
                        string EncodName = ou_detected_encoding.Length > 0 ? ou_detected_encoding : DetectFileEncoding(curPath);
                        string[] otoText = File.ReadAllLines(curPath, EncodingUtils.GetEncoding(EncodName));
                        List<string> otoAliases= new List<string>();
                        foreach (string otoLine in otoText)
                        {
                            if (otoLine.Trim().StartsWith("#")) continue;
                            Oto oto = new Oto();
                            oto.BaseDirs.AddRange(otoPathMap[0..(otoPathMap.Length - 1)]);
                            string[] spL1 = otoLine.Split('=');
                            if (spL1.Length < 2) continue;
                            oto.Wav = spL1[0];
                            string otoLabel = spL1[1];
                            string[] spL2 = otoLabel.Split(',');
                            if (spL2.Length < 6) continue;

                            //LP,FL,RP,PR,OV
                            oto.Alias = spL2[0].Trim();
                            if (oto.Alias.Length == 0)
                            {
                                oto.Alias = Path.GetFileNameWithoutExtension(oto.Wav);
                            }
                            oto.Offset = double.Parse(otofmt(spL2[1]));
                            oto.Consonant = double.Parse(otofmt(spL2[2]));
                            oto.Cutoff = double.Parse(otofmt(spL2[3]));
                            oto.Preutter = double.Parse(otofmt(spL2[4]));
                            oto.Overlap = double.Parse(otofmt(spL2[5]));

                            oto.FileEncoding = "";
                            if (!oto.isVaild(VoiceBankPath))
                            {
                                oto.FileEncoding = EncodName; 
                                if (!oto.isVaild(VoiceBankPath))
                                {
                                    oto.FileEncoding = "Shift-JIS";
                                }
                            }

                            if (oto.Alias.Length > 0) otoAliases.Add(oto.Alias);
                            vb.Otos.Add(oto);
                        }
                        {
                            try
                            {
                                vb.PrefixPairs = UVoiceBankPrefixAnalysiser.AddPair(vb.PrefixPairs, UVoiceBankPrefixAnalysiser.AnalysisPrefixFromAlias(otoAliases),"[*]");
                            }
                            catch { }
                        }
                    }
                    catch {; }
                }
            }

            vb.DefaultLyric = "a";
            if (vb.FindSymbol("あ") != null || vb.FindSymbol("あ",60) != null || vb.FindSymbol("- あ", 60) != null) vb.DefaultLyric = "あ";
            try
            {
                vb.Serialize(cache);
            }
            catch { }
            vb.vbBasePath = VoiceBankPath;
            return vb;
        }
    }
}
