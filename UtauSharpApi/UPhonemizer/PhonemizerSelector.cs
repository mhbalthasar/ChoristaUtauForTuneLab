using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer
{
    public class PhonemizerSelector
    {
        private static List<Type> ExtendedPhonemizer = new List<Type>();
        public static void LoadExtendedPhonemizer(string SearchFolder)
        {
            if (!Directory.Exists(SearchFolder)) return;
            List<string> dllFiles=new List<string>(Directory.GetFiles(SearchFolder,"*.dll"));
            string[] subDir = Directory.GetDirectories(SearchFolder);
            foreach (string dir in subDir) dllFiles.AddRange(Directory.GetFiles(dir,"*.dll"));
            foreach (string dllFile in dllFiles)
            {
                try
                {
                    Assembly ass = Assembly.LoadFrom(dllFile);
                    var types=ass.GetTypes().Where(t=>t.GetInterfaces().Contains(typeof(IPhonemizer)) && !t.IsAbstract);
                    ExtendedPhonemizer.AddRange(types);
                }
                catch {; }
            }
        }

        public static IPhonemizer GuessPhonemizer(VoiceBank vb)
        {
            IPhonemizer curIP;
            if ((curIP = new MocaloidPhonemizer(vb)).ProcessAble(vb)) return curIP;
            if ((curIP = new PresampCVVCPhonemizer(vb)).ProcessAble(vb)) return curIP;
            if ((curIP = new DefaultVCVJapanesePhonemizer()).ProcessAble(vb)) return curIP;
            if ((curIP = new DefaultJapanesePhonemizer()).ProcessAble(vb)) return curIP;
            foreach(var cp in ExtendedPhonemizer)
            {
                object? cObj = Activator.CreateInstance(cp);
                if (cObj == null) continue;
                curIP = (IPhonemizer)cObj;
                if (curIP.ProcessAble(vb)) return curIP;
            }
            return new DefaultPhonemizer();
        }

        public static IPhonemizer BuildPhonemizer(string phonemizer)
        {
            return new DefaultPhonemizer();
        }
    }
}
