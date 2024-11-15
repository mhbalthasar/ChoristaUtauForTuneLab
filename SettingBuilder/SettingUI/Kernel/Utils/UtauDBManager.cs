using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ude;

namespace ChoristaUtau.SettingUI.Kernel.Utils
{
    internal class UtauDBManager
    {
        public static Dictionary<string, Tuple<string, string>> UtauVoiceBankInfo { get; set; } = new Dictionary<string, Tuple<string, string>>();
        private static string GetVBName(string VoiceBankPath, out string EncodingName)
        {
            string DetectFileEncoding(string filePath)
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

            EncodingName = "";
            string vbName = "";
            bool bOUOverlay_Name = false;
            string ou_detected_encoding = "";

            //OverlayOU
            if (File.Exists(Path.Combine(VoiceBankPath, "character.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    Dictionary<object, object> ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(VoiceBankPath, "character.yaml")));
                    if (ouMap.TryGetValue("name", out object nameValue))
                    {
                        vbName = (string)nameValue;
                        bOUOverlay_Name = true;
                    }

                    if (ouMap.TryGetValue("text_file_encoding", out object encodingValue)) ou_detected_encoding = (string)encodingValue;
                    EncodingName = ou_detected_encoding;
                }
                catch {; }
            }

            //ReadVBName
            if (!bOUOverlay_Name)
            {
                vbName = Path.GetFileNameWithoutExtension(VoiceBankPath);
                string EncodName = ou_detected_encoding.Length > 0 ? ou_detected_encoding : DetectFileEncoding(Path.Combine(VoiceBankPath, "character.txt"));
                GetVBNameWithEncoding(VoiceBankPath, EncodName, out string? enName, out string? vN);
                if (enName != null) EncodingName = enName;
                if (vN != null) vbName = vN;
            }
            return vbName;
        }
        public static void GetVBNameWithEncoding(string VoiceBankPath, string EncodingInput, out string? EncodingName, out string? vbName)
        {
            Encoding GetEncoding(string EncodingName)
            {
                if (EncodingName == "UTF8") return Encoding.UTF8;
                if (EncodingName == "ASCII") return Encoding.ASCII;
                if (EncodingName == "<Other>") return Encoding.ASCII;
                if (EncodingName == "Unicode") return Encoding.Unicode;
                return CodePagesEncodingProvider.Instance.GetEncoding(EncodingName);
            }
            EncodingName = null;
            vbName = null;
            try
            {
                if (File.Exists(Path.Combine(VoiceBankPath, "character.txt")))
                {
                    string[] characterText = File.ReadAllLines(Path.Combine(VoiceBankPath, "character.txt"), GetEncoding(EncodingInput));// Path.Combine(VoiceBankPath, "character.txt"));
                    foreach (string line in characterText)
                    {
                        if (line.ToLower().StartsWith("name="))
                        {
                            vbName = line.Substring(5);
                            EncodingName = EncodingInput;
                            break;
                        }
                    }
                }
            }
            catch {; }
        }
        public static void GetVBOverlayName(string VoiceBankPath, out string? vbName)
        {
            vbName = null;
            if (File.Exists(Path.Combine(VoiceBankPath, "character.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    Dictionary<object, object> ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(VoiceBankPath, "character.yaml")));
                    if (ouMap.TryGetValue("name", out object nameValue))
                    {
                        vbName = (string)nameValue;
                    }
                }
                catch {; }
            }
        }


        public static List<string> FindVB(string DirPath, int SearchDeeply = 10)
        {
            List<string> ret = new List<string>();
            if (!Directory.Exists(DirPath)) return ret;
            if (File.Exists(Path.Combine(DirPath, "character.txt")))
            {
                ret.Add(DirPath);
                return ret;
            }
            if (SearchDeeply == 0) return ret;
            foreach (string subDir in Directory.GetDirectories(DirPath))
            {
                ret.AddRange(FindVB(subDir, SearchDeeply - 1));
            }
            return ret;
        }
        public static void UpdateDBList(string[] searchDirs)
        {
            Dictionary<string, Tuple<string, string>> GetVoiceBanks()
            {
                List<string> allPath = new List<string>();
                allPath.AddRange(searchDirs);
                List<string> VBPaths = new List<string>();
                foreach (string path in allPath) VBPaths.AddRange(FindVB(path, 10));

                Dictionary<string, Tuple<string, string>> VBL = new Dictionary<string, Tuple<string, string>>();

                //LoadEachVoiceBank
                foreach (string vbp in VBPaths)
                {
                    try
                    {
                        string VBName = GetVBName(vbp, out string enc);
                        int ord = 0;
                        while (true)
                        {
                            if (!(VBL.ContainsKey(VBName))) break;
                            ord++;
                            VBName = string.Format("{0} #{1}", VBName, ord);
                        }
                        VBL.Add(vbp, new Tuple<string, string>(VBName, enc));
                    }
                    catch {; }
                }

                return VBL;
            }
            UtauVoiceBankInfo = GetVoiceBanks();
        }
    }
}
