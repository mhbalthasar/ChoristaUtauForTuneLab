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

        static bool isWow64 = false;
        static string? strWinePath = null;
        static string? strWine64Path = null;
        static string? strBox86Path = null;
        static string? strBox64Path = null;
        static string? strLatxPath = null;
        static string? strLatx64Path = null;
        public static string winePath { get {
                if (strWinePath != null) return strWinePath;
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string DepUserWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine", "bin", "wine");
                string DepUserWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine", "bin", "wine64");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    DepUserWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine.app", "Contents", "MacOS", "wine");
                    DepUserWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine.app", "Contents", "Resources", "wine", "bin", "wine");
                    if (File.Exists(DepUserWine64 + "64")) DepUserWine64 = DepUserWine64 + "64"; 
                    if (File.Exists(DepUserWine + "64")) DepUserWine = DepUserWine + "64"; 
                }
                if (Path.Exists(DepUserWine64))
                {
                    strWinePath = DepUserWine64;
                    isWow64 = true;
                    return strWinePath;
                }
                if (Path.Exists(DepUserWine))
                {
                    strWinePath = DepUserWine;
                    isWow64 = true;
                    return strWinePath;
                }
                string userWine= Path.Combine(UserProfile, "containers", "wine", "bin", "wine");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    userWine = Path.Combine(UserProfile, "containers", "wine.app", "Contents", "MacOS", "wine");
                }
                if (Path.Exists(userWine))
                {
                    strWinePath = userWine;
                    return strWinePath;
                }
                string localWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "wine", "bin", "wine");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string localWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", "osx", "wine.app", "Contents", "Resources", "wine", "bin", "wine");
                    if (File.Exists(localWine64)) localWine = localWine64;
                    else
                    {
                        localWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", "osx", "wine.app", "Contents", "MacOS", "wine");
                    }
                }
                if (Path.Exists(localWine))
                {
                    strWinePath = localWine;
                    return strWinePath;
                }
                if (Path.Exists("/usr/local/bin/wine32on64")) return "/usr/local/bin/wine32on64";
                if (Path.Exists("/usr/local/bin/wine")) return "/usr/local/bin/wine";
                if (Path.Exists("/usr/bin/wine")) return "/usr/bin/wine";
                if (Path.Exists("/usr/bin/deepin-wine")) return "/usr/bin/deepin-wine";
                if (Path.Exists("/usr/lib/deepin-wine/wine")) return "/usr/lib/deepin-wine/wine";
                if (Path.Exists("/usr/share/deepin-wine/wine")) return "/usr/share/deepin-wine/wine";
                if (Path.Exists("/usr/lib/i386-linux-gnu/deepin-wine5/wine")) return "/usr/lib/i386-linux-gnu/deepin-wine5/wine";
                if (Path.Exists("/opt/deepin-wine8-stable/bin/wine")) return "/opt/deepin-wine8-stable/bin/wine";
                if (Path.Exists("/opt/deepin-wine6-stable/bin/wine")) return "/opt/deepin-wine6-stable/bin/wine";
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
                    if (strWinePath.Length == 0 || !File.Exists(strWinePath))
                    {
                        strWinePath = FindWineByWhichCommand("deepin-wine");
                    }
                    return strWinePath;
                }
                return "";
            }
        }
        public static string winePath64
        {
            get
            {
                if (strWine64Path != null) return strWine64Path;
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                string DepUserWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine", "bin", "wine");
                string DepUserWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine", "bin", "wine64");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    DepUserWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine.app", "Contents", "MacOS", "wine");
                    DepUserWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "wine.app", "Contents", "Resources", "wine", "bin", "wine");
                    if (File.Exists(DepUserWine64 + "64")) DepUserWine64 = DepUserWine64 + "64";
                    if (File.Exists(DepUserWine + "64")) DepUserWine = DepUserWine + "64";
                }
                if (Path.Exists(DepUserWine64))
                {
                    strWinePath = DepUserWine64;
                    return strWinePath;
                }
                if (Path.Exists(DepUserWine))
                {
                    strWinePath = DepUserWine;
                    return strWinePath;
                }

                string userWine = Path.Combine(UserProfile, "containers", "wine", "bin", "wine64");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    userWine = Path.Combine(UserProfile, "containers", "wine.app", "Contents", "MacOS", "wine64");
                }
                if (Path.Exists(userWine))
                {
                    strWinePath = userWine;
                    return strWinePath;
                }
                string localWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "wine", "bin", "wine64");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string localWine64 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", "osx", "wine.app", "Contents", "Resources", "wine", "bin", "wine");
                    if (File.Exists(localWine64 + "64")) localWine64 = localWine64 + "64";
                    if (File.Exists(localWine64)) localWine = localWine64;
                    else
                    {
                        localWine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", "osx", "wine.app", "Contents", "MacOS", "wine");
                        if (File.Exists(localWine + "64")) localWine = localWine + "64";
                    }
                }
                if (Path.Exists(localWine))
                {
                    strWinePath = localWine;
                    return strWinePath;
                }
                if (Path.Exists("/usr/local/bin/wine32on64")) return "/usr/local/bin/wine32on64";
                if (Path.Exists("/usr/local/bin/wine64")) return "/usr/local/bin/wine64";
                if (Path.Exists("/usr/bin/wine64")) return "/usr/bin/wine64";
                if (Path.Exists("/usr/bin/deepin-wine64")) return "/usr/bin/deepin-wine64";
                if (Path.Exists("/usr/lib/deepin-wine/wine64")) return "/usr/lib/deepin-wine/wine64";
                if (Path.Exists("/usr/share/deepin-wine/wine64")) return "/usr/share/deepin-wine/wine64";
                if (Path.Exists("/usr/lib/i386-linux-gnu/deepin-wine5/wine64")) return "/usr/lib/i386-linux-gnu/deepin-wine5/wine64";
                if (Path.Exists("/opt/deepin-wine8-stable/bin/wine64")) return "/opt/deepin-wine8-stable/bin/wine64";
                if (Path.Exists("/opt/deepin-wine6-stable/bin/wine64")) return "/opt/deepin-wine6-stable/bin/wine64";
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
                    strWinePath = FindWineByWhichCommand("wine64");
                    return strWinePath;
                }
                //If is linux
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    strWinePath = FindWineByWhichCommand("wine64");
                    if (strWinePath.Length == 0 || !File.Exists(strWinePath))
                    {
                        strWinePath = FindWineByWhichCommand("deepin-wine64");
                    }
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
                string DepUserBox86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "box86", "box86");
                if (Path.Exists(DepUserBox86))
                {
                    strBox86Path = DepUserBox86;
                    return strBox86Path;
                }
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string userBox86 = Path.Combine(UserProfile, "containers", "box86", "box86");
                if (Path.Exists(userBox86))
                {
                    strBox86Path = userBox86;
                    return strBox86Path;
                }
                string localBox86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX)?"osx":"linux", "box86", "box86");
                if (Path.Exists(localBox86))
                {
                    strBox86Path = localBox86;
                    return strBox86Path;
                }
                if (Path.Exists("/usr/local/bin/box86")) return "/usr/local/bin/box86";
                if (Path.Exists("/usr/bin/box86")) return "/usr/bin/box86";
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
                    if(strBox86Path.Length==0 || !File.Exists(strBox86Path))
                    {
                        strBox86Path = FindWineByWhichCommand("spark-box86");
                    }
                    return strBox86Path;
                }
                strBox86Path = "";
                return "";
            }
        }
        public static string box64Path
        {
            get
            {
                if (strBox64Path != null) return strBox64Path;
                string DepUserBox86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "WineRunners", "box86", "box64");
                if (Path.Exists(DepUserBox86))
                {
                    strBox64Path = DepUserBox86;
                    return strBox64Path;
                }
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string userBox86 = Path.Combine(UserProfile, "containers", "box86", "box64");
                if (Path.Exists(userBox86))
                {
                    strBox64Path = userBox86;
                    return strBox64Path;
                }
                string localBox86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "box86", "box64");
                if (Path.Exists(localBox86))
                {
                    strBox64Path = localBox86;
                    return strBox64Path;
                }
                if (Path.Exists("/usr/local/bin/box64")) return "/usr/local/bin/box64";
                if (Path.Exists("/usr/bin/box64")) return "/usr/bin/box64";
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
                    strBox64Path = FindWineByWhichCommand("box64");
                    if (strBox64Path.Length == 0 || !File.Exists(strBox64Path))
                    {
                        strBox64Path = FindWineByWhichCommand("spark-box64");
                    }
                    return strBox64Path;
                }
                strBox64Path = "";
                return "";
            }
        }

        public static string latxPath
        {
            get
            {
                string latxFile = "latx-i386";//X86 "latx_x86_64 is 64bit
                if (strLatxPath != null) return strLatxPath;
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string userLatx86 = Path.Combine(UserProfile, "containers", "latx", latxFile);
                if (Path.Exists(userLatx86))
                {
                    strLatxPath = userLatx86;
                    return strLatxPath;
                }
                string localLatx86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "latx", latxFile);
                if (Path.Exists(localLatx86))
                {
                    strLatxPath = localLatx86;
                    return strLatxPath;
                }
                if (Path.Exists("/usr/local/bin/"+ latxFile)) return "/usr/local/bin/"+ latxFile;
                if (Path.Exists("/usr/bin/"+ latxFile)) return "/usr/bin/"+ latxFile;
                if(Path.Exists("/usr/gnemul/latx-i386")) return "/usr/bin/latx-i386";
                if (Path.Exists("/usr/gnemul/lat-i386")) return "/usr/bin/lat-i386";
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
                    strLatxPath = FindWineByWhichCommand("latx-i386");
                    if (strLatxPath.Length == 0 || !File.Exists(strLatxPath))
                    {
                        strLatxPath = FindWineByWhichCommand("lat-i386-static");
                    }
                    if (strLatxPath.Length == 0 || !File.Exists(strLatxPath))
                    {
                        strLatxPath = FindWineByWhichCommand("lat-i386");
                    }
                    return strLatxPath;
                }
                strLatxPath = "";
                return "";
            }
        }
        public static string latx64Path
        {
            get
            {
                string latxFile = "latx-x86_64";//X86 "latx_x86_64 is 64bit
                if (strLatx64Path != null) return strLatx64Path;
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string userLatx86 = Path.Combine(UserProfile, "containers", "latx", latxFile);
                if (Path.Exists(userLatx86))
                {
                    strLatx64Path = userLatx86;
                    return strLatx64Path;
                }
                string localLatx86 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "containers", RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux", "latx", latxFile);
                if (Path.Exists(localLatx86))
                {
                    strLatx64Path = localLatx86;
                    return strLatx64Path;
                }
                if (Path.Exists("/usr/local/bin/" + latxFile)) return "/usr/local/bin/" + latxFile;
                if (Path.Exists("/usr/bin/" + latxFile)) return "/usr/bin/" + latxFile;
                if (Path.Exists("/usr/gnemul/latx-x86_64")) return "/usr/bin/latx-x86_64";
                if (Path.Exists("/usr/gnemul/lat-x86_64")) return "/usr/bin/lat-x86_64";
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
                    strLatx64Path = FindWineByWhichCommand("latx-x86_64");
                    if (strLatx64Path.Length == 0 || !File.Exists(strLatx64Path))
                    {
                        strLatx64Path = FindWineByWhichCommand("lat-x86_64-static");
                    }
                    if (strLatx64Path.Length == 0 || !File.Exists(strLatx64Path))
                    {
                        strLatx64Path = FindWineByWhichCommand("lat-x86_64");
                    }
                    return strLatx64Path;
                }
                strLatx64Path = "";
                return "";
            }
        }
        public WineProcessInfo GetWineInfo(string exePath,bool b64bit=false)
        {
            WineProcessInfo ret = new WineProcessInfo();
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            b64bit = b64bit || isWow64;
            string PrefixDir = Path.Combine(UserProfile, ".TuneLab", "WinePrefixs", b64bit ? "x64" : "x86");
            try
            {
                if (!Directory.Exists(PrefixDir)) Directory.CreateDirectory(PrefixDir);
                ret.envs.Add("WINEPREFIX", PrefixDir);//专用容器
            }
            catch {; }
            Console.WriteLine("WINEPREFIX:{0}", PrefixDir);
            Console.WriteLine("WINEARCH:{0}", b64bit ? "win64" : "win32");
            ret.envs.Add("WINEARCH", b64bit ? "win64" : "win32");

            string wPath = winePath;
            if (b64bit && File.Exists(winePath64)) wPath = winePath64;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WineProcessInfo() { exePath = exePath };
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && (
                    RuntimeInformation.ProcessArchitecture == Architecture.Arm ||
                    RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                    RuntimeInformation.ProcessArchitecture == Architecture.Armv6
                    ))
                {
                    WineProcessInfo ret_arm = ret;
                    ret_arm.exePath = b64bit ? box64Path : (File.Exists(box86Path)?box86Path:box64Path);
                    if (wPath == winePath64 && wPath.EndsWith("64")) ret_arm.exePath = box64Path;
                    ret_arm.args.Add(wPath);
                    ret_arm.args.Add(exePath);
                    ret_arm.envs.Add("BOX64_LD_LIBRARY_PATH",Path.GetDirectoryName(box64Path));
                    ret_arm.envs.Add("BOX86_LD_LIBRARY_PATH", Path.GetDirectoryName(box86Path));
                    return ret_arm;
                }
            }
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.LoongArch64)
                {
                    WineProcessInfo ret_arm = ret;
                    ret_arm.exePath = b64bit ? latx64Path : (File.Exists(latxPath)?latxPath:latx64Path);
                    ret_arm.args.Add(wPath);
                    ret_arm.args.Add(exePath);
                    return ret_arm;
                }
            }
            ret.exePath = wPath;
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
