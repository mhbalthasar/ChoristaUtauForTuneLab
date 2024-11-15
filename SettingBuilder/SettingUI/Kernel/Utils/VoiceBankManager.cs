using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ChoristaUtau.SettingUI.Kernel.Utils.VoiceBankManager;

namespace ChoristaUtau.SettingUI.Kernel.Utils
{
    internal class VoiceBankManager
    {
        public class VoiceBankInfo
        {
            public string Name = "";
            public string Path = "";
            public string CompID = "";
            public string InstallDir = "";
        }
        public static List<VoiceBankInfo> VoiceBankLoader(List<string> VoiceDirList)
        {
            List<VoiceBankInfo> VoiceBankList = new List<VoiceBankInfo>();
            object mLock = new object();
           bool endsWithExt(string inPath)
            {
                var ext = Path.GetExtension(inPath).ToLower();
                if (ext.EndsWith("ddi") || ext.EndsWith("ddb")) return true;
                return false;
            }
            void CheckSubFolder(string inPath)
            {
                DirectoryInfo di = new DirectoryInfo(inPath);
                if (!di.Exists) return;
                var files = di.GetDirectories();
                foreach (var file in files)
                {
                    AddBank(file.FullName);
                }
            }
            string findFileName(string inPath)
            {
                DirectoryInfo di = new DirectoryInfo(inPath);
                if (!di.Exists) return "";
                foreach (var x in di.GetFiles())
                {
                    string ext = x.Extension.ToLower();
                    if (ext != ".ddb") continue;
                    string fddi = x.FullName;
                    string fddb = x.FullName.Substring(0, x.FullName.Length - 1) + "i";
                    if (Path.Exists(fddb))
                    {
                        return fddb;
                    }
                }
                return "";
            }
            void AddBank(string inPath)
            {
                string fileName = "";
                string compID = "";
                if (endsWithExt(inPath))
                {
                    fileName = Path.GetFileName(inPath);
                    inPath = Path.GetDirectoryName(inPath);
                }
                else
                {
                    CheckSubFolder(inPath);
                    fileName = findFileName(inPath);
                }
                if (fileName.Length < 4) return;
                compID = Path.GetFileName(inPath);
                inPath = Path.GetDirectoryName(inPath);
                if (compID.Length != 16) return;
                string str = string.Format("{0}[{1}]", Path.GetFileNameWithoutExtension(fileName), compID);
                lock (mLock)
                {
                    VoiceBankList.Add(new VoiceBankInfo() { Name = Path.GetFileNameWithoutExtension(fileName),Path=Path.GetDirectoryName(fileName),InstallDir=inPath,CompID=compID });
                }
            }
            string[] WindowsREG()
            {
                List<string> vbSearchPath = new List<string>();
                string[] loadRegPath(string regPath)
                {
                    List<string> result = new List<string>();
                    try
                    {
                        using (RegistryKey lmKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                        {
                            var regRoot = lmKey.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadSubTree);
                            if (regRoot != null)
                            {
                                Parallel.ForEach(regRoot.GetSubKeyNames().Where(p => (p.Length == 16 && p.ToUpper() == p)), (compID) =>
                                {
                                    var reg = regRoot.OpenSubKey(compID);
                                    if (reg != null)
                                    {
                                        string path = string.Format("{0}", reg.GetValue("PATH", reg.GetValue("INSTALLDIR", "")));
                                        if (path.Length > 0)
                                        {
                                            result.Add(path);
                                        }
                                        reg.Close();
                                    }
                                });
                                regRoot.Close();
                            }
                        }
                    }
                    catch { }
                    return result.ToArray();
                }
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\VOCALOID5\\Voice\\Components"));

                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VOCALOID4\\DATABASE41"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VOCALOID4\\DATABASE"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\POCALOID4\\DATABASE41"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\POCALOID4\\DATABASE"));

                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VOCALOID3\\DATABASE\\VOICE3"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VCLDASGN3\\DATABASE\\VOICE3"));

                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VOCALOID3\\DATABASE\\VOICE2"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VOCALOID2\\DATABASE\\VOICE"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\VCLDASGN3\\DATABASE\\VOICE2"));
                vbSearchPath.AddRange(loadRegPath("SOFTWARE\\WOW6432Node\\POCALOID2\\DATABASE\\VOICE"));

                vbSearchPath = vbSearchPath.Distinct().ToList();

                return vbSearchPath.ToArray();
            }
            List<string> VDL = new List<string>();
            VDL.AddRange(VoiceDirList);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { VDL.AddRange(WindowsREG()); VDL = VDL.Distinct().ToList(); }
            Parallel.ForEach(VDL, (item) => {
                if (item.StartsWith("REG32:\\") || item.StartsWith("REG64:\\") || !Path.Exists(item)) return;
                AddBank(item);
            });
            VoiceBankList = VoiceBankList.DistinctBy(p=>p.CompID).OrderBy(p => p.Name).ToList();

            return VoiceBankList;
        }
    }
}
