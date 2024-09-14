using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UtaubaseForTuneLab.Utils
{
    public class WineProcessInfo
    {
        public string exePath { get; set; } = "";
        public List<string> args { get; set; } = new List<string>();
        public Dictionary<string, string> envs { get; set; } = new Dictionary<string, string>();
    }
    public class WineHelper
    {
        public static bool UnderWine { get => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }

        static string? strWinePath = null;
        static string? strBox86Path = null;
        public static string winePath { get {
                if (strWinePath != null) return strWinePath;
                string localWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "wine", "bin", "wine");
                if (Path.Exists("/usr/local/bin/wine32on64")) return "/usr/local/bin/wine32on64";
                if (Path.Exists("/usr/local/bin/wine")) return "/usr/local/bin/wine";
                if (Path.Exists("/usr/bin/wine")) return "/usr/bin/wine";
                if(Path.Exists(localWine))
                {
                    strWinePath = localWine;
                    return strWinePath;
                }
                string FindWineByWhichCommand(string fileName)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "which", // 使用which命令查找Wine
                        Arguments = fileName,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        return output.Trim(); // 返回找到的Wine路径
                    }
                }
                //If is linux
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    strWinePath = FindWineByWhichCommand("wine");
                    return strWinePath;
                }
                //If is linux
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    strWinePath = FindWineByWhichCommand("wine");
                    return strWinePath;
                }
                return "";
            }
        }
        public static string box86Path
        {
            get
            {
                if (strBox86Path != null) return strBox86Path;
                string localBox86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX)?"osx":"linux", "box86", "box86");
                if (Path.Exists("/usr/local/bin/box86")) return "/usr/local/bin/box86";
                if (Path.Exists("/usr/bin/box86")) return "/usr/bin/box86";
                if (Path.Exists(localBox86))
                {
                    strWinePath = localBox86;
                    return strWinePath;
                }
                string FindWineByWhichCommand(string fileName)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "which", // 使用which命令查找Wine
                        Arguments = fileName,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        return output.Trim(); // 返回找到的Wine路径
                    }
                }
                //If is linux
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    strBox86Path = FindWineByWhichCommand("box86");
                    return strBox86Path;
                }
                strBox86Path = "";
                return "";
            }
        }

        public WineProcessInfo GetWineInfo(string exePath,bool b64bit=false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WineProcessInfo() { exePath = exePath };
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && (
                RuntimeInformation.ProcessArchitecture==Architecture.Arm ||
                RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                RuntimeInformation.ProcessArchitecture == Architecture.Armv6
                ))
            {
                WineProcessInfo ret_arm = new WineProcessInfo();
                ret_arm.exePath = box86Path;
                ret_arm.args.Add(winePath);
                ret_arm.args.Add(exePath);
                return ret_arm;
            }
            WineProcessInfo ret = new WineProcessInfo();
            ret.exePath = winePath;
            ret.args.Add(exePath);
            return ret;
        }

        public Process CreateWineProcess(string exePath, List<string> args, string WorkDir = "", bool NoWindow = true,bool b64bit=false)
        {
            WineProcessInfo wpi = GetWineInfo(exePath,b64bit);
            Process p = new Process();
            p.StartInfo.FileName = wpi.exePath;
            p.StartInfo.CreateNoWindow = NoWindow;
            p.StartInfo.UseShellExecute = false;
            if (WorkDir != "") p.StartInfo.WorkingDirectory = WorkDir;
            foreach (string arg in wpi.args) { p.StartInfo.ArgumentList.Add(arg); };
            foreach (string arg in args) { p.StartInfo.ArgumentList.Add(arg); };
            foreach (var kv in wpi.envs)
            {
                if (p.StartInfo.Environment.ContainsKey(kv.Key)) p.StartInfo.Environment[kv.Key] = kv.Value;
                else p.StartInfo.Environment.Add(kv.Key, kv.Value);
            }
            return p;
        }
    }
}
