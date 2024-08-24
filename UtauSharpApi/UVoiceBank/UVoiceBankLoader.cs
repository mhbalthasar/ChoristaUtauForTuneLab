using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ude;
using UtauSharpApi.Utils;

namespace UtauSharpApi.UVoiceBank
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
        private static string GetVBHash(string VoiceBankPath, List<string[]> OtoFiles)
        {
            List<string> subHash = new List<string>();
            if (File.Exists(Path.Combine(VoiceBankPath, "prefix.map"))) { subHash.Add(GetFileHash(Path.Combine(VoiceBankPath, "prefix.map"))); }
            if (File.Exists(Path.Combine(VoiceBankPath, "character.txt"))) { subHash.Add(GetFileHash(Path.Combine(VoiceBankPath, "character.txt"))); }
            foreach (string[] otoPathMap in OtoFiles)
            {
                string otoPath = Path.Combine(otoPathMap);
                string curPath = Path.Combine(VoiceBankPath, otoPath);
                if (File.Exists(curPath))
                {
                    subHash.Add(GetFileHash(curPath));
                }
            }
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
        public static VoiceBank LoadVoiceBank(string VoiceBankPath)
        {
            List<string[]> otoFiles = SearchOto(VoiceBankPath);
            string hash = GetVBHash(VoiceBankPath, otoFiles);
            string cache = Path.Combine(VoiceBankPath, "uvoicebank.protobuf");
            if (File.Exists(cache))
            {
                var vbc = VoiceBank.Deserialize(cache);
                vbc.vbBasePath = VoiceBankPath;
                if (vbc.CacheHash == hash) 
                    return vbc;
            }

            VoiceBank vb = new VoiceBank();
            vb.CacheHash = hash;

            //ReadVBName
            vb.Name = Path.GetFileNameWithoutExtension(VoiceBankPath);
            if (File.Exists(Path.Combine(VoiceBankPath, "character.txt")))
            {
                string EncodName = DetectFileEncoding(Path.Combine(VoiceBankPath, "character.txt"));
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

            //ReadVBPrefix
            if (File.Exists(Path.Combine(VoiceBankPath, "prefix.map")))
            {
                string EncodName = DetectFileEncoding(Path.Combine(VoiceBankPath, "prefix.map"));
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
                        vb.PrefixMap[nn] = new PrefixItem() { prefix = prefixStr, suffix = suffixStr };
                    }
                }
            }

            //LoadOto
            List<string> otoDirList = new List<string>();
            foreach (string[] otoPathMap in otoFiles)
            {
                string otoPath = Path.Combine(otoPathMap);
                string curPath = Path.Combine(VoiceBankPath, otoPath);
                if (File.Exists(curPath))
                {
                    string EncodName = DetectFileEncoding(curPath);
                    string[] otoText = File.ReadAllLines(curPath, EncodingUtils.GetEncoding(EncodName));
                    foreach (string otoLine in otoText)
                    {
                        Oto oto = new Oto();
                        oto.BaseDirs.AddRange(otoPathMap[0..(otoPathMap.Length - 1)]);
                        string[] spL1 = otoLine.Split('=');
                        oto.Wav = spL1[0];
                        string otoLabel = spL1[1];
                        string[] spL2 = otoLabel.Split(',');

                        //LP,FL,RP,PR,OV
                        oto.Alias = spL2[0];
                        oto.Offset = double.Parse(spL2[1]);
                        oto.Consonant = double.Parse(spL2[2]);
                        oto.Cutoff = double.Parse(spL2[3]);
                        oto.Preutter = double.Parse(spL2[4]);
                        oto.Overlap = double.Parse(spL2[5]);

                        oto.FileEncoding = EncodName;

                        vb.Otos.Add(oto);
                    }
                }
            }

            vb.DefaultLyric = "a";
            if (vb.FindSymbol("あ") != null) vb.DefaultLyric = "あ";

            vb.Serialize(cache);
            vb.vbBasePath = VoiceBankPath;
            return vb;
        }
    }
}
