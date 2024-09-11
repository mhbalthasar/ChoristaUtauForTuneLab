using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using TLXPackageHelper;

internal class Program
{

    private static void Main(string[] args)
    {
        Console.WriteLine("TLX插件打包助手");
        Package("ChoristaUtau", @"..\..\..\", @"..\..\..\Output\");
    }

    private static void Package(string PluginName,string ProjectDir = "", string OutputDir = ".\\")
    {
        string pfmDir = "";
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pfmDir = "win_x64";
        }
        string GetProjectDir = Assembly.GetExecutingAssembly().Location.Split("\\bin\\Release\\")[0].Split("\\bin\\Debug\\")[0];
        if (ProjectDir.StartsWith(".")) { ProjectDir = Path.Combine(GetProjectDir, ProjectDir); }
        string CompileOutputDir = Path.Combine(System.Environment.GetEnvironmentVariable("AppData"), "TuneLab", "Extensions", PluginName);
        ProjectDir = Path.GetFullPath(ProjectDir);

        Dictionary<string,string> CopiesDirectory = new Dictionary<string, string>()
        {
            {Path.Combine(ProjectDir,"dependencies",pfmDir), "" },
            {Path.Combine(ProjectDir,"ChoristaUtau.Phonemizers","OpenUtau.PhonemeAdapter","dependencies",pfmDir), Path.Combine("phonemizers","OpenUtauBuiltinAdapter") },
        };
    /*
  <OutputPath>$(AppData)\TuneLab\Extensions\VOCALOID5</OutputPath>
  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
     */
        string srcDescriptionPath = Path.Combine(ProjectDir, "ChoristaUtauLoader", "description.json");
        FileInfo fi = new FileInfo(srcDescriptionPath);
        string ff = fi.FullName;
        ExtensionDescription? description = null;
        if (File.Exists(srcDescriptionPath))
        {
            try
            {
                string desContent = "";
                using(FileStream fs=new FileStream(srcDescriptionPath, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    desContent = sr.ReadToEnd();
                }
                description = JsonSerializer.Deserialize<ExtensionDescription>(desContent);
            }
            catch {; }
        }
        #if DEBUG
                string DebugSign = "-Debug";
        #else
                string DebugSign = "";
        #endif
        string FileName = description==null?"OutputTlx.tlx":string.Format("{0}forTuneLab-{1}-v{2}{3}.tlx", description.name, description.platforms[0],description.version,DebugSign);
        string OutputFile = Path.Combine(OutputDir,FileName);
        if (OutputFile.StartsWith(".")) { OutputFile = Path.GetFullPath(Path.Combine(GetProjectDir, OutputFile)); }

        Dictionary<string, string> FilePath = SearchFile(CompileOutputDir);
        foreach(var cDirs in CopiesDirectory)
        {
            string rc = cDirs.Key;
            string tg = cDirs.Value;
            Dictionary<string, string> ffc = SearchFile(rc,null, tg, true);
            foreach(var cm in ffc)
            {
                if (!FilePath.ContainsKey(cm.Key)) FilePath.Add(cm.Key, cm.Value);
            }
        }
        if(!FilePath.ContainsKey("description.json"))
        {
            FilePath.Add("description.json", srcDescriptionPath);
        }

        ZipTo(FilePath, OutputFile);
        Console.WriteLine("Done!Packaged in "+OutputFile);
    }

    private static Dictionary<string, string> SearchFile(string BaseDir,Dictionary<string,string>? BaseDictionary=null, string DirPrefix = "", bool isDepends=false)
    {
        List<string> disableExt = new List<string> { ".pdb" };
        List<string> disableDir = new List<string> { "runtimes",Path.Combine("phonemizers", "OpenUtauBuiltinAdapter", "runtimes") };
        List<string> disabledFile = new List<string>() { "description.json", "TuneLab.Extensions.Voices.dll", "TuneLab.Base.dll", Path.Combine("phonemizers", "OpenUtauBuiltinAdapter", "ChoristaUtauApi.dll") };

        Dictionary<string, string> ret = BaseDictionary == null ? new Dictionary<string, string>() : BaseDictionary;
        if (System.IO.Path.Exists(BaseDir))
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(BaseDir);
            foreach (FileInfo f in di.GetFiles())
            {
                string pf = Path.Combine(DirPrefix, f.Name);
                if (!isDepends && disabledFile.Contains(pf))continue;//这个交给Depends
                string file = f.FullName;
                string ext = f.Extension;
                if (!isDepends && disableExt.Contains(ext)) continue;
                if (ret.ContainsKey(pf)) ret[pf] = file; else ret.Add(pf, file);
            }
            foreach (DirectoryInfo d in di.GetDirectories())
            {
                string pf = Path.Combine(DirPrefix, d.Name);
                if (!isDepends && disableDir.Contains(pf)) continue;//这个交给Depends
                string dir = d.FullName;
                ret = SearchFile(dir, ret, pf);
            }
        }
        return ret;
    }

    private static void ZipTo(Dictionary<string,string> fileList,string outputFile)
    {
        if (System.IO.File.Exists(outputFile)) { System.IO.File.Delete(outputFile);}
        if (!System.IO.File.Exists(System.IO.Path.GetDirectoryName(outputFile))) { System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFile)); };
        using (FileStream zipFile = new FileStream(outputFile, FileMode.Create))
        {
            using (ZipArchive zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create))
            {
                foreach (var kv in fileList)
                {
                    if (File.Exists(kv.Value))
                    {
                        ZipArchiveEntry entry = zipArchive.CreateEntry(kv.Key);
                        using (Stream sourceStream = new FileStream(kv.Value, FileMode.Open))
                        {
                            using (Stream destinationStream = entry.Open())
                            {
                                sourceStream.CopyTo(destinationStream);
                            }
                        }
                    }
                }
            }
        }
    }
}