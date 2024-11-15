using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtau.SettingUI.Kernel.Utils
{
    internal class CrossOSUtils
    {
        public static void OpenFolder(string folderPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = folderPath,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = folderPath,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = folderPath,
                    UseShellExecute = true
                });
            }
            else
            {
                Console.WriteLine("不支持的操作系统。");
            }
        }
    }
}
