

using System.IO;
using System.Text;
using Ude;

internal class Program
{
    static int OtoSearchDeeply = 3;
    private static List<string[]> SearchOto(string basePath, int deeply = 0)
    {
        if (OtoSearchDeeply > -1 && deeply >= OtoSearchDeeply) return new List<string[]>();

        List<string[]> ret = new List<string[]>();
        if (File.Exists(Path.Combine(basePath, "oto.ini"))) ret.Add(["oto.ini"]);
        foreach (string subdir in Directory.GetDirectories(basePath))
        {
            string curP = Path.GetFileName(subdir);
            var sub = SearchOto(subdir, deeply >= 0 ? deeply + 1 : -1);
            foreach (string[] srr in sub)
            {
                List<string> newStr = new List<string>();
                newStr.Add(curP);
                newStr.AddRange(srr);
                ret.Add(newStr.ToArray());
            }
        }
        return ret;
    }
    private List<string> FindVB(string DirPath, int SearchDeeply = 10)
    {
        List<string> ret = new List<string>();
        if (!Directory.Exists(DirPath)) return ret;
        if (File.Exists(Path.Combine(DirPath, "character.txt")))
        {
            ret.Add(DirPath);
            return ret;
        }
        if (SearchDeeply == 0) return ret;
        foreach (string subDir in Directory.GetDirectories(DirPath))
        {
            ret.AddRange(FindVB(subDir, SearchDeeply - 1));
        }
        return ret;
    }
    private static void Main(string[] args)
    {
        string codeCode = "";
        string basePath = "D:\\UtauDB\\David^^";

        string character = Path.Combine(basePath,"character.txt");
        List<string> otoList = SearchOto(basePath).Select(pp=>Path.Combine(basePath, Path.Combine(pp))).ToList();
        var kk = "";
    }
}