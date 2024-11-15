using ChoristaUtau.SettingUI.Kernel.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ChoristaUtau.SettingUI
{
    internal static class Worker
    {
        public static double CacheSize = 0;

        public static int VoiceDirSelectedIndex = -1;
        public static List<string> VoiceDirList = new List<string>() { "" };        
        public static int VoiceBankSelectedIndex = -1;
        public static List<string> VoiceBankList => UtauDBManager.UtauVoiceBankInfo.Values.Select(p => (string)p.Item1).ToList();

        public static string GuiTitle = "ChoristaUtau Extension Configs";

        private static List<string> VoiceDirFileData = new List<string>() { "" };
        
        public static int NotVoiceDirFileDataLen = 0;
        public static void UpdateVoiceDirs()
        {
            string oldName = "";
            if (VoiceDirList.Count > VoiceDirSelectedIndex && VoiceDirSelectedIndex >= 0) oldName = VoiceDirList[VoiceDirSelectedIndex];
            VoiceDirList.Clear();

            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            {
                VoiceDirList = new List<string>()
                {
                    Path.Combine(UserProfile,"utauvbs"),
                    Path.Combine(UserProfile,".TuneLab","ChoristaUtau", "voicedb")
                };
                NotVoiceDirFileDataLen = VoiceDirList.Count();
                if (!Directory.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"))) Directory.CreateDirectory(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"));
                if (File.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt")))
                {
                    VoiceDirFileData.Clear();
                    VoiceDirFileData.AddRange(File.ReadAllLines(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt")).Select(p => p.Trim()).Where(p => p.Length > 0 && Directory.Exists(p)));
                    VoiceDirFileData = VoiceDirFileData.Where(p => p.Length > 0).ToList();
                    VoiceDirList.AddRange(VoiceDirFileData);
                }
                else
                    File.WriteAllText(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt"), "");
            }

            string oldVBName = "";
            if (VoiceBankList.Count > VoiceBankSelectedIndex && VoiceBankSelectedIndex >= 0) oldVBName = VoiceBankList[VoiceBankSelectedIndex];
            UtauDBManager.UpdateDBList(VoiceDirList.ToArray());
            VoiceBankSelectedIndex= Array.FindIndex(VoiceBankList.ToArray(), item => item == oldVBName);
            if (VoiceBankSelectedIndex != -1) LoadVoiceBankData(VoiceBankSelectedIndex);

            VoiceDirSelectedIndex = Array.FindIndex(VoiceDirList.ToArray(), item => item == oldName);
        }

        public static void AddDir(string path)
        {
            if (Path.Exists(path))
            {
                if (VoiceDirList.Contains(path)) return;
                {
                    VoiceDirFileData.Add(path);
                    string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    File.WriteAllLines(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt"), VoiceDirFileData);
                    UpdateVoiceDirs();
                }
            }
        }
        public static void AddDir()
        {
            var ret= NativeFileDialogSharp.Dialog.FolderPicker();
            if (ret.IsOk)
            {
                    AddDir(ret.Path);
            }
        }

        public static void RemoveDir(object index)
        {
            if (typeof(int) != index.GetType()) return;
            string content = VoiceDirFileData[(int)index];
            if(content == VoiceDirList[VoiceDirSelectedIndex].ToString())
            {
                VoiceDirFileData.RemoveAt((int)index);
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                File.WriteAllLines(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt"), VoiceDirFileData);
                UpdateVoiceDirs();
            }
        }
        public static string PickUtauVoiceBank(out string error)
        {
            error = "";
            var ret = NativeFileDialogSharp.Dialog.FileOpen("txt");
            if (ret.IsOk)
            {
                string pathdir = Path.GetDirectoryName(ret.Path);
                string character = Path.Combine(pathdir, "character.txt");
                if (!Path.Exists(character)) { error = "Please Select the character.txt file to install"; return null; }
                return pathdir;
            }
            return "";
        }



        public static void NavigateDBPath(object index)
        {
            if (typeof(int) != index.GetType()) return;
            if ((int)index < 0 || (int)index > VoiceBankList.Count) return;
            var dir = UtauDBManager.UtauVoiceBankInfo.Keys.ToArray()[(int)index];
            if (Path.Exists(dir))
            {
                CrossOSUtils.OpenFolder(dir);
            }
        }

        
        public static void UpdateCacheSize()
        {
            var dirs=TempManager.GetTempDirs();
            long size=TempManager.GetDirSize(dirs);
            CacheSize= (double)size / (1024 * 1024 * 1024);
        }

        public static void ClearCache()
        {
            try
            {
                var dirs = TempManager.GetTempDirs();
                foreach (string dir in dirs)
                {
                    if (Directory.Exists(dir)) {
                        Directory.Delete(dir, true);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    }
                }
            }
            catch {; }
        }
        public static void KillAllResamplers()
        {
            void killOne(string exeName)
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.ArgumentList.Add("/c");
                p.StartInfo.ArgumentList.Add("taskkill");
                p.StartInfo.ArgumentList.Add("/f");
                p.StartInfo.ArgumentList.Add("/im");
                p.StartInfo.ArgumentList.Add(exeName);
                p.Start();
                p.WaitForExit();
            }
            killOne("moresampler.exe");
            killOne("doppeltler32.exe");
        }
        public static void CleanAllUVoiceBank()
        {
            Task.Run(() =>
            {
                List<string> allPath = new List<string>();
                allPath.AddRange(VoiceDirList);

                List<string> VBPaths = new List<string>();
                foreach (string path in allPath) VBPaths.AddRange(UtauDBManager.FindVB(path, 10));

                foreach (string str in VBPaths)
                {
                    string tmpFile = Path.Combine(str, "uvoicebank.protobuf");
                    if (File.Exists(tmpFile)) try
                        {
                            File.Delete(tmpFile);
                        }
                        catch { }
                }
            });
        }


        public static string CurrentVBInfo_OverlayedName = "";
        public static string CurrentVBInfo_DetectedName = "";
        public static int CurrentVBInfo_EncodingIndex = 0;
        public static int CurrentVBInfo_PhonemizerIndex = 0;
        public static string GetVBInfo_Path(int vb_index,int maxWidth=0)
        {
            static string WrapTextWithCJK(string text, int maxWidth)
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                var result = new StringBuilder();
                int currentWidth = 0;

                foreach (char c in text)
                {
                    // 计算字符宽度
                    int charWidth = IsCJKCharacter(c) ? 2 : 1;

                    // 如果宽度超出，换行
                    if (currentWidth + charWidth > maxWidth)
                    {
                        result.Append("\n");
                        currentWidth = 0;
                    }

                    // 添加字符到结果
                    result.Append(c);
                    currentWidth += charWidth;
                }

                return result.ToString();
            }
            static bool IsCJKCharacter(char c)
            {
                // 检查字符是否在 CJK Unicode 范围内
                return (c >= 0x4E00 && c <= 0x9FFF) || // 中文
                       (c >= 0x3040 && c <= 0x309F) || // 平假名
                       (c >= 0x30A0 && c <= 0x30FF) || // 片假名
                       (c >= 0xAC00 && c <= 0xD7AF);   // 韩文
            }

            if (typeof(int) != vb_index.GetType()) return "";
            if ((int)vb_index < 0 || (int)vb_index > VoiceBankList.Count) return "";
            var dir = UtauDBManager.UtauVoiceBankInfo.Keys.ToArray()[(int)vb_index];
            if (Path.Exists(dir))
            {
                if (maxWidth>0)
                {
                    return WrapTextWithCJK(dir, maxWidth);
                }
                return dir;
            }
            return "";
        }
        public static int GetVBInfo_EncodingIndex(int vb_index)
        {
            string path = GetVBInfo_Path(vb_index);
            var cr=UtauDBManager.UtauVoiceBankInfo[path];
            string enc = cr.Item2.ToLower()
                        .Replace("shift-jis", "shift_jis");
            if (enc.Length == 0) return 0;
            else if (enc == "utf8" || enc == "utf-8") return 2;
            else if (enc == "ascii") return 0;
            else if (enc == "unicode") return 4;
            else
            {
                var cp = EncodingNameList.IndexOf(enc.ToUpper());
                if (cp == -1) return 1;
                else
                    return cp;
            }
        }
        public static string GetVBInfo_EncodingName(int index)
        {
            if (CurrentVBInfo_EncodingIndex > 0 && CurrentVBInfo_EncodingIndex < EncodingNameList.Count) return EncodingNameList[CurrentVBInfo_EncodingIndex];
            return EncodingNameList[0];
        }
        public static string GetVBInfo_PhonemizerName(int index)
        {
            if (CurrentVBInfo_PhonemizerIndex > 0 && CurrentVBInfo_PhonemizerIndex < PhonemizerList.Count) return PhonemizerList[CurrentVBInfo_PhonemizerIndex];
            return PhonemizerList[0];
        }
        public static void OnVBEncodingChange(int index)
        {
            if (VoiceBankSelectedIndex < 0 || VoiceBankSelectedIndex >= VoiceBankList.Count) return;
            var dbpath = GetVBInfo_Path(VoiceBankSelectedIndex);
            UtauDBManager.GetVBNameWithEncoding(dbpath, GetVBInfo_EncodingName(index), out string? enN, out string? VN);
            if (VN != null)
            {
                CurrentVBInfo_DetectedName = VN;
            }
            else
            {
                CurrentVBInfo_DetectedName = "";
            }
        }
        public static void OnVBPhonemizerChange(int index)
        {

        }
        static void updatePhonemizer(int vb_index)
        {
            if (vb_index != VoiceBankSelectedIndex) return;
            if (vb_index < 0 || vb_index >= VoiceBankList.Count) return;
            try
            {
                var dbpath = GetVBInfo_Path(vb_index);
                var pth = Path.Combine(dbpath, "phonemizer.txt");
                if (File.Exists(pth))
                {
                    string sk = File.ReadAllLines(pth)[0].Trim();
                    int id = PhonemizerList.IndexOf(sk);
                    if (id < 1)
                    {
                        CurrentVBInfo_PhonemizerIndex = 1;
                        return;
                    }
                    else
                    {
                        CurrentVBInfo_PhonemizerIndex = id;
                        return;
                    }
                }
            }
            catch {; }
            CurrentVBInfo_PhonemizerIndex = 0;
        }
        public static void LoadVoiceBankData(int vb_index)
        {
            if (vb_index != VoiceBankSelectedIndex) return;
            if (vb_index < 0 || vb_index >= VoiceBankList.Count) return;
            {
                UtauDBManager.GetVBOverlayName(GetVBInfo_Path(vb_index), out string overlayName);
                if (overlayName != null) CurrentVBInfo_OverlayedName = overlayName; else CurrentVBInfo_OverlayedName = "";
            }
            {
                updatePhonemizer(vb_index);
            }
            {
                CurrentVBInfo_EncodingIndex = GetVBInfo_EncodingIndex(vb_index);
                OnVBEncodingChange(CurrentVBInfo_EncodingIndex);
            }
        }
        public static void Do_VB_RebuildUVB(int vb_index)
        {
            if (vb_index != VoiceBankSelectedIndex) return;
            if (vb_index < 0 || vb_index >= VoiceBankList.Count) return;
            string tmpFile = Path.Combine(GetVBInfo_Path(vb_index), "uvoicebank.protobuf");
            if (File.Exists(tmpFile))
                try
                {
                    File.Delete(tmpFile);
                }
                catch { }

        }
        public static void Do_VB_SaveChange(int vb_index)
        {
            if (vb_index != VoiceBankSelectedIndex) return;
            if (vb_index < 0 || vb_index >= VoiceBankList.Count) return;
            SaveItem(vb_index);
        }
        static void SaveItem(int vb_index)
        {
            var dbpath = GetVBInfo_Path(vb_index);
            void savePhonemizer(string key)
            {
                try
                {
                    var pth = Path.Combine(dbpath, "phonemizer.txt");
                    if (key.Trim() == "" || key.ToLower().Trim() == "autoselect")
                    {
                        if (File.Exists(pth)) File.Delete(pth);
                    }else if(key.ToLower().Trim()=="<other>")
                    {
                        ;
                    }
                    else
                    {

                        File.WriteAllLines(pth, new string[1] { key.Trim() });
                    }
                }
                catch {; }
            }
            savePhonemizer(GetVBInfo_PhonemizerName(CurrentVBInfo_PhonemizerIndex));
            Dictionary<object, object> ouMap = new Dictionary<object, object>();
            if (File.Exists(Path.Combine(dbpath, "character.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(dbpath, "character.yaml")));
                }
                catch {; }
            }
            if (CurrentVBInfo_EncodingIndex > 1)
            {
                string enc = EncodingNameList[CurrentVBInfo_EncodingIndex].ToLower();
                if (enc == "shift_jis") enc = "shift-jis";
                if (ouMap.ContainsKey("text_file_encoding"))
                    ouMap["text_file_encoding"] = enc;
                else
                    ouMap.Add("text_file_encoding", enc);
            }
            else
            {
                if (ouMap.ContainsKey("text_file_encoding")) ouMap.Remove("text_file_encoding");
            }
            if (CurrentVBInfo_OverlayedName.Trim() != "")
            {
                string tnn = CurrentVBInfo_OverlayedName.Trim();
                if (ouMap.ContainsKey("name"))
                    ouMap["name"] = tnn;
                else
                    ouMap.Add("name", tnn);
            }
            else
            {
                if (ouMap.ContainsKey("name")) ouMap.Remove("name");
            }
            try
            {
                if (ouMap.Count == 0)
                {
                    if (File.Exists(Path.Combine(dbpath, "character.yaml")))
                    {
                        File.Delete(Path.Combine(dbpath, "character.yaml"));
                    }
                }
                else
                {
                    YamlDotNet.Serialization.Serializer s = new YamlDotNet.Serialization.Serializer();
                    string content = s.Serialize(ouMap);
                    File.WriteAllText(Path.Combine(dbpath, "character.yaml"), content);
                }
            }
            catch {; }
            UpdateVoiceDirs();
        }

        public static List<string> EncodingNameList = new List<string>(new[] { "AutoDetect", "<Other>", "UTF8", "UTF16", "Unicode" }.Concat(CodePagesEncodingProvider.Instance.GetEncodings().Select(p => p.Name.ToUpper()).Order()));
        public static List<string> PhonemizerList = new List<string>()
        {
            //Export From UtauList
            "AutoSelect","<Other>","Auto CVVC(Presamp.ini)","Auto Mocaloid","Japanese Romaji","Japanese VCV","Mocaloid English (Alpha)","Whole Word (CV)","[OU]Chinese (Presamp)","[OU]Chinese CVV Extend","[OU]Chinese CVVC","[OU]Chinese Syo Cantonese","[OU]English Arpasing","[OU]English Arpasing+","[OU]English VCCV","[OU]English via JPN VB","[OU]English XSampa","[OU]French CVVC","[OU]French Diphone","[OU]French VCCV","[OU]German Diphone","[OU]German VCCV","[OU]Italian CVVC","[OU]Italian Syllable","[OU]Japanese (Presamp) Hiragana","[OU]Japanese (Presamp) Romaji","[OU]Japanese CVVC Hiragana","[OU]Japanese CVVC Romaji","[OU]Japanese VCV Hiragana","[OU]Japanese VCV Romaji","[OU]Korean CBNN","[OU]Korean CV","[OU]Korean CVC","[OU]Korean CVCCV","[OU]Korean CVVC","[OU]Korean VCV","[OU]Korean via JPN VB","[OU]Polish CVC","[OU]Protuguese CVC Brazilian","[OU]Russian CVC","[OU]Russian VCCV","[OU]Spanish Makkusan","[OU]Spanish Syllable","[OU]Spanish VCCV","[OU]Spanish via JPN VB","[OU]Thai VCCV","[OU]Vietnamese CVVC","[OU]Vietnamese VCV","[OU]Vietnamese VINA"
        };
    }
}
