using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtau.SettingUI.Kernel.Utils
{
    internal class TempManager
    {
        static string WineX64TempDir()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string PrefixDir = Path.Combine(UserProfile, ".TuneLab", "WinePrefixs", "x64");
            if (UserProfile.StartsWith("//home//") || UserProfile.StartsWith("//Users//"))
            {
                try
                {
                    string usrName = UserProfile.Split("//")[1];
                    string tryPath = PrefixDir + "/drive_c/users/" + usrName + "/Temp/ChoristaUtau";
                    if (Directory.Exists(tryPath)) return tryPath;
                }
                catch {; }
            }
            return "";
        }
        static string WineX86TempDir()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string PrefixDir = Path.Combine(UserProfile, ".TuneLab", "WinePrefixs", "x86");
            if (UserProfile.StartsWith("//home//") || UserProfile.StartsWith("//Users//"))
            {
                try
                {
                    string usrName = UserProfile.Split("//")[1];
                    string tryPath = PrefixDir + "/drive_c/users/" + usrName + "/Temp/ChoristaUtau";
                    if (Directory.Exists(tryPath)) return tryPath;
                }
                catch {; }
            }
            return "";
        }
        static string SysTempDir()
        {
            var tmp=Path.Combine(Path.GetTempPath(), "ChoristaUtau");
            if(Directory.Exists(tmp)) return tmp;
            return "";
        }

        public static string[] GetTempDirs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return [SysTempDir()];
            List<string> dirs = new List<string>();
            if (SysTempDir() != "") dirs.Add(SysTempDir());
            if (WineX64TempDir() != "") dirs.Add(WineX64TempDir());
            if (WineX86TempDir() != "") dirs.Add(WineX86TempDir());
            return dirs.ToArray();
        }

        public static long GetDirSize(string[] dirs)
        {
            object o = new object();
            long ret = 0;
            Parallel.ForEach(dirs, (dir) => {
                var v = GetDirectorySize(dir);
                lock (o)
                {
                    ret += v;
                }
            });
            return ret;
        }

        static long GetDirectorySize(string path)
        {
            long totalSize = 0;

            // 获取目录下的所有文件
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            // 遍历文件并累加大小
            foreach (string file in files)
            {
                totalSize += new FileInfo(file).Length; // 获取文件大小并累加
            }

            return totalSize;
        }
    }
}
