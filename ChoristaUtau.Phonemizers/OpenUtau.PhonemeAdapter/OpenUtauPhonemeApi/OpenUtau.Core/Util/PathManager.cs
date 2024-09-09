using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.Core {

    public class PathManager : SingletonBase<PathManager> {
        public PathManager() {
            /*if (OS.IsMacOS()) {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                DataPath = Path.Combine(userHome, "Library", "OpenUtau");
                CachePath = Path.Combine(userHome, "Library", "Caches", "OpenUtau");
                HomePathIsAscii = true;
                try {
                    // Deletes old cache.
                    string oldCache = Path.Combine(DataPath, "Cache");
                    if (Directory.Exists(oldCache)) {
                        Directory.Delete(oldCache, true);
                    }
                } catch { }
            } else if (OS.IsLinux()) {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(dataHome)) {
                    dataHome = Path.Combine(userHome, ".local", "share");
                }
                DataPath = Path.Combine(dataHome, "OpenUtau");
                string cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
                if (string.IsNullOrEmpty(cacheHome)) {
                    cacheHome = Path.Combine(userHome, ".cache");
                }
                CachePath = Path.Combine(cacheHome, "OpenUtau");
                HomePathIsAscii = true;
            } else {
                string exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                IsInstalled = File.Exists(Path.Combine(exePath, "installed.txt"));
                if (!IsInstalled) {
                    DataPath = exePath;
                } else {
                    string dataHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    DataPath = Path.Combine(dataHome, "OpenUtau");
                }
                CachePath = Path.Combine(DataPath, "Cache");
                HomePathIsAscii = true;
                var etor = StringInfo.GetTextElementEnumerator(DataPath);
                while (etor.MoveNext()) {
                    string s = etor.GetTextElement();
                    if (s.Length != 1 || s[0] >= 128) {
                        HomePathIsAscii = false;
                        break;
                    }
                }
            }*/
            DataPath=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public string DataPath { get; private set; }
        public string PluginsPath => Path.Combine(DataPath, "Plugins");
        public string DictionariesPath => Path.Combine(DataPath, "Dictionaries");
    }
}
