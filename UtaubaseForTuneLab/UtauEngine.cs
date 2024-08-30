using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;
using UtauSharpApi.UPhonemizer;
using UtauSharpApi.UVoiceBank;

namespace UtaubaseForTuneLab
{
    public abstract class UtauEngine : IVoiceEngine
    {
        public List<string> UVoiceBankSearchPath = new List<string>()
        {
            @"F:\F\G\VocalUtau\VocalUtau\bin\Debug\voicedb",
            @"C:\Program Files (x86)\VoiceDB\Iroha_V4\BFPG63T4DEZMRFCD",
            "C:\\Program Files\\Common Files\\VOCALOID5\\Voicelib\\BD79E492NWWK3DDF\\Ext",
            "D:\\UtauDB\\"
        };


        public const string PitchTransitionTimeID = "PitchTransitionTime";
        public readonly static NumberConfig PitchTransitionTimeConfig = new() { DefaultValue = 0.12, MinValue = 0, MaxValue = 0.2 };
        public const string NoteFlagsID = "Flags";
        public readonly static StringConfig NoteFlagsConfig = new() { DefaultValue = string.Empty };

        public IReadOnlyOrderedMap<string, VoiceSourceInfo> VoiceInfos { get; set; }=new OrderedMap<string, VoiceSourceInfo>();
        private Dictionary<string, VoiceBank> VoiceBanks = new Dictionary<string, VoiceBank>();

        protected IRenderEngine? RenderEngine { get; set; } = null;

        public IVoiceSource CreateVoiceSource(string id)
        {
            if (RenderEngine == null)
                throw new Exception("Render Engine is not inited!");
            if (!VoiceBanks.ContainsKey(id))
                throw new Exception("Unloaded VoiceBank!");
            return new UtauVoiceSource(
                RenderEngine,
                VoiceBanks[id],
                PhonemizerSelector.GuessPhonemizer(VoiceBanks[id]),
                new OrderedMap<string, AutomationConfig>(),
                new OrderedMap<string, IPropertyConfig>() {
                    { PitchTransitionTimeID, PitchTransitionTimeConfig}
                },
                new OrderedMap<string, IPropertyConfig>() {
                    { NoteFlagsID,NoteFlagsConfig}
                });
        }
        public void Destroy()
        {
            return;
        }

        private List<string> FindVB(string DirPath,int SearchDeeply=10)
        {
            List<string> ret = new List<string>();
            if(File.Exists(Path.Combine(DirPath,"character.txt")))
            {
                ret.Add(DirPath);
                return ret;
            }
            if (SearchDeeply == 0) return ret;
            foreach(string subDir in Directory.GetDirectories(DirPath))
            {
                ret.AddRange(FindVB(subDir, SearchDeeply - 1));
            }
            return ret;
        }

        public abstract bool InitRenderEngine(string enginePath, out string? error);
        public bool Init(string enginePath, out string? error)
        {
            if(!InitRenderEngine(enginePath, out error))return false;
            if (RenderEngine == null) { error = "Utau Render engine not impl!"; return false; }
            error = "";

            List<string> VBPaths = new List<string>();
            foreach(string path in UVoiceBankSearchPath)
            {
                VBPaths.AddRange(FindVB(path,10));
            }
            
            foreach(string vbp in VBPaths)
            {
                VoiceBank vb = UVoiceBankLoader.LoadVoiceBank(vbp);
                ((OrderedMap<string, VoiceSourceInfo>)VoiceInfos).Add("UTAU_" + vb.Name, new VoiceSourceInfo() { Name = vb.Name });
                VoiceBanks.Add("UTAU_" + vb.Name, vb);
            }
            return true;
        }
    }
}
