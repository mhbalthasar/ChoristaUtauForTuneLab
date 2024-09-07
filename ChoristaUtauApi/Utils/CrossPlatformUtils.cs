using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.Utils
{
    public static class CrossPlatformUtils
    {
        /// <summary>
        /// This Function is use for keep the path is base on Windows
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string KeepWindows(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return path;
            if (path.StartsWith("/"))
            {//Convert To Wine
                return "Z:\\"+path.Replace("/","\\");
            }
            else
            {
                path=Path.Combine(Directory.GetCurrentDirectory(), path);
                return "Z:\\" + path.Replace("/", "\\");
            }
        }
    }
}
