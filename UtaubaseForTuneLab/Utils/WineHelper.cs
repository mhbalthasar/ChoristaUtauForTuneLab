using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public WineProcessInfo GetWineInfo(string exePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WineProcessInfo() { exePath = exePath };
            WineProcessInfo ret = new WineProcessInfo();
            ret.exePath = "wine";
            ret.args.Add(exePath);
            return ret;
        }

        public Process CreateWineProcess(string exePath, List<string> args, string WorkDir = "", bool NoWindow = true)
        {
            WineProcessInfo wpi = GetWineInfo(exePath);
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
