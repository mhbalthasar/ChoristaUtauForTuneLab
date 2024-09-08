using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChoristaUtauApi.UVoiceBank;

namespace ChoristaUtauApi.UPhonemizer
{
    public class PhonemizerSelector
    {
        private static List<KeyValuePair<string, Type>> PhonemizerList = new List<KeyValuePair<string, Type>>()
        {
            PhonemizerKeyPair(typeof(MocaloidPhonemizer)),
            PhonemizerKeyPair(typeof(PresampCVVCPhonemizer)),
            PhonemizerKeyPair(typeof(DefaultVCVJapanesePhonemizer)),
            PhonemizerKeyPair(typeof(DefaultJapanesePhonemizer)),
            PhonemizerKeyPair(typeof(DefaultPhonemizer)),
        };
        private static KeyValuePair<string, Type> PhonemizerKeyPair(Type type)
        {
            return new KeyValuePair<string, Type>(
                type.GetCustomAttribute<PhonemeizerAttribute>().PhonemizerType,
                type);
        }

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
                    foreach(var type in types)
                    {
                        if (type.GetCustomAttribute(typeof(PhonemeizerAttribute), false) != null)
                        {
                            var kVP = PhonemizerKeyPair(type);
                            if (PhonemizerList.Where(p=>p.Key==kVP.Key).Count()>0)continue;//去重
                            PhonemizerList.Add(kVP);
                        }
                    }
                }
                catch {; }
            }
        }

        private static object mObjectLocker = new object();//为了防止初始化Phonemizer时候出问题，加线程安全锁

        public static IPhonemizer GuessPhonemizer(VoiceBank vb)
        {
            string lastKey = GetLastPhonemizer(vb);
            if (lastKey != "")
            {
                IPhonemizer? lastPhon = BuildPhonemizer(lastKey,vb);
                if (lastPhon != null) return lastPhon;
            }
            IPhonemizer curIP;
            foreach (var pPair in PhonemizerList)
            {
                var cp = pPair.Value;
                object? cObj = null;
                lock (mObjectLocker)
                {
                    cObj = Activator.CreateInstance(cp, new object[1] { vb });
                }
                if (cObj == null) continue;
                curIP = (IPhonemizer)cObj;
                if (curIP.ProcessAble()) 
                    return curIP;
            }
            return new DefaultPhonemizer();
        }

        public static List<string> GetAllPhonemizerKeys()
        {
            return PhonemizerList.Select(p => p.Key).Order().ToList();
        }

        public static IPhonemizer? BuildPhonemizer(string Key,VoiceBank vb)
        {
            var phonType =  PhonemizerList.Where(p=>p.Key==Key).Select(p => p.Value).FirstOrDefault();
            if (phonType == null) return null;
            object? cObj = null;
            lock (mObjectLocker)
            {
                cObj = Activator.CreateInstance(phonType, new object[1] { vb });
            }
            if (cObj == null) return null;
            return (IPhonemizer)cObj;
        }

        private static string GetLastPhonemizer(VoiceBank vb)
        {
            try
            {
                var pth = Path.Combine(vb.vbBasePath, "phonemizer.txt");
                if (File.Exists(pth))
                {
                    return File.ReadAllLines(pth)[0].Trim();
                }
            }
            catch {; }
            return "";
        }
        public static void SaveLastPhonemizer(string key, VoiceBank vb)
        {
            try
            {
                var pth = Path.Combine(vb.vbBasePath, "phonemizer.txt");
                if (key == "")
                {
                    if (File.Exists(pth)) File.Delete(pth);
                }
                else
                {

                    File.WriteAllLines(pth, new string[1] { key });
                }
            }
            catch {; }
        }
    }
}
