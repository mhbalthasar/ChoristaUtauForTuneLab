using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab.Utils;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UVoiceBank;

namespace UtaubaseForTuneLab
{
    public abstract class UtauEngine : IVoiceEngine
    {
        public const string MinSegmentSpacingID = "MinSegmentSpacing";
        public readonly static NumberConfig MinSegmentSpacingConfig = new() { DefaultValue = 0, MinValue = 0, MaxValue = 2 };
        public const string PitchTransitionTimeID = "PitchTransitionTime";
        public readonly static NumberConfig PitchTransitionTimeConfig = new() { DefaultValue = 0.12, MinValue = 0, MaxValue = 0.2 };
        public const string PhonemizerSelectorID = "Phonemizer";

        public const string EarlyStartID = "EarlyStart";
        public readonly static NumberConfig EarlyStartConfig = new () { DefaultValue = 0, MinValue = -1000, MaxValue = 1000,IsInterger=true };
        public const string PrefixPairID = "PrefixPair";
        public const string VelocityID = "Velocity";
        public readonly static NumberConfig VelocityConfig = new() { DefaultValue = 1, MinValue = 0, MaxValue = 2};
        //XSynthesis
        public const string XTrack_XPrefixKeyID = "XTrack Synthesis Percent";
        public readonly static AutomationConfig XTrack_XPrefixKeyConfig = new("XSP", 0, 0, 1, "#73ACE5");
        //AttrackFixArgs
        public const string AttrackID = "AttrackVolume";
        public readonly static AutomationConfig AttrackConfig = new("ATK", 1, 0, 1, "#73ACE5");
        public const string ReleaseID = "ReleaseVolume";
        public readonly static AutomationConfig ReleaseConfig = new("RLE", 1, 0, 1, "#73ACE5");
        //OtoFixedArgs
        public const string OtoPreutterFixedID = "OtoCorrected:Preutter";
        public readonly static AutomationConfig OtoPreutterFixedConfig = new("PRE", 0, -100, 100, "#73ACE5");
        public const string OtoOverlapFixedID = "OtoCorrected:Overlap";
        public readonly static AutomationConfig OtoOverlapFixedConfig = new("OVL", 0, -100, 100, "#73ACE5");
        public const string OtoLeftBFixedID = "OtoCorrected:Leftbound";
        public readonly static AutomationConfig OtoLeftBConfig = new("LBD", 0, -100, 100, "#73ACE5");
        public const string OtoFixedLengthFixedID = "OtoCorrected:FixedLength";
        public readonly static AutomationConfig OtoFixedLengthFixedConfig = new("FXL", 0, -100, 100, "#73ACE5");
        public const string OtoRightBFixedID = "OtoCorrected:Rightbound";
        public readonly static AutomationConfig OtoRightBFixedConfig = new("RBD", 0, -100, 100, "#73ACE5");

        public const string XTrack_PrefixPairID = "XTrack:PrefixPair";

        public IReadOnlyOrderedMap<string, VoiceSourceInfo> VoiceInfos { get; set; }=new OrderedMap<string, VoiceSourceInfo>();
        private Dictionary<string, VoiceBank> VoiceBanks = new Dictionary<string, VoiceBank>();

        protected IRenderEngine? RenderEngine { get; set; } = null;

        public IVoiceSource CreateVoiceSource(string id)
        {
            if (RenderEngine == null)
                throw new Exception("Render Engine is not inited!");
            if (!VoiceBanks.ContainsKey(id))
                throw new Exception("Unloaded VoiceBank!");
            List<string> PrefixPair = new List<string>() { "AutoSelect" };
            PrefixPair.AddRange(VoiceBanks[id].GetPrefixPairs());

            OrderedMap<string, IPropertyConfig> XTrackNoteProperties = new OrderedMap<string, IPropertyConfig>()
            {
                {XTrack_PrefixPairID,new EnumConfig(PrefixPair)}
            };

            List<string> PhonemizerEnumList = new List<string>();
            {
                PhonemizerEnumList.Add("AutoSelect");
                var ok=PhonemizerSelector.GetAllPhonemizerKeys();
                PhonemizerEnumList.AddRange(ok.Where(p => !p.StartsWith("[OU]")));
                PhonemizerEnumList.AddRange(ok.Where(p => p.StartsWith("[OU]")));
            }
            

            return new UtauVoiceSource(
                RenderEngine,
                VoiceBanks[id],
                new OrderedMap<string, AutomationConfig>()
                {
                    {XTrack_XPrefixKeyID, XTrack_XPrefixKeyConfig },
                    {AttrackID,AttrackConfig },
                    {ReleaseID,ReleaseConfig },
                    {OtoPreutterFixedID,OtoPreutterFixedConfig },
                    {OtoOverlapFixedID,OtoOverlapFixedConfig },
                    {OtoFixedLengthFixedID,OtoFixedLengthFixedConfig },
                    {OtoLeftBFixedID,OtoLeftBConfig },
                    {OtoRightBFixedID,OtoRightBFixedConfig }
                }.Combine(RenderEngine.AutomationConfigs, false),
                new OrderedMap<string, IPropertyConfig>() {
                    { PitchTransitionTimeID, PitchTransitionTimeConfig},
                    { MinSegmentSpacingID, MinSegmentSpacingConfig},
                    { PhonemizerSelectorID,new EnumConfig(PhonemizerEnumList)}
                }.Combine(RenderEngine.PartProperties)
                 .Combine(AudioEffect.AudioEffectHelper.PartProperties),
                new OrderedMap<string, IPropertyConfig>() {
                    { VelocityID,VelocityConfig },
                    { EarlyStartID,EarlyStartConfig },
                    { PrefixPairID, new EnumConfig(PrefixPair) }
                }.Combine(RenderEngine.NoteProperties)
                 .Combine(XTrackNoteProperties)
                ); 
        }
        public void Destroy()
        {
            return;
        }

        private List<string> FindVB(string DirPath,int SearchDeeply=10)
        {
            List<string> ret = new List<string>();
            if (!Directory.Exists(DirPath)) return ret;
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
            if (!InitRenderEngine(enginePath, out error)) return false;
            if (RenderEngine == null) { error = "Utau Render engine not impl!"; return false; }
            error = "";

            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            //Load Extended Phonemizer
            {
                List<string> UPhonemizerSearchDir = new List<string>(){
                Path.Combine(enginePath, "phonemizers"),
                Path.Combine(UserProfile,".TuneLab","ChoristaUtau", "phonemizers")
            };
                foreach (string searchDir in UPhonemizerSearchDir) ChoristaUtauApi.UPhonemizer.PhonemizerSelector.LoadExtendedPhonemizer(searchDir);
            }
            //Load VoiceBanks
            {
                List<string> UVoiceBankSearchPath = new List<string>()
                {
                    Path.Combine(enginePath, "voicedb"),
                    Path.Combine(UserProfile,".TuneLab","ChoristaUtau", "voicedb"),
                    Path.Combine(UserProfile,"utauvbs")
                };
                if (!Directory.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau")))Directory.CreateDirectory(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"));
                if (File.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt")))
                    UVoiceBankSearchPath.AddRange(File.ReadAllLines(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt")).Select(p => p.Trim()).Where(p => p.Length > 0 && Directory.Exists(p)));
                else
                    File.WriteAllText(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt"), "");

                //SearchAllDir
                List<string> VBPaths = new List<string>();
                foreach (string path in UVoiceBankSearchPath) VBPaths.AddRange(FindVB(path, 10));

                //LoadEachVoiceBank
                foreach (string vbp in VBPaths)
                {
                    try
                    {
                        VoiceBank vb = UVoiceBankLoader.LoadVoiceBank(vbp);
                        string VBName = vb.Name;
                        int ord = 0;
                        while (true)
                        {
                            if (!(VoiceBanks.ContainsKey(RenderEngine.EngineUniqueString + "_" + VBName))) break;
                            ord++;
                            VBName = string.Format("{0} #{1}", vb.Name, ord);
                        }
                        ((OrderedMap<string, VoiceSourceInfo>)VoiceInfos).Add(RenderEngine.EngineUniqueString + "_" + VBName, new VoiceSourceInfo() { Name = vb.Name });
                        VoiceBanks.Add(RenderEngine.EngineUniqueString + "_" + VBName, vb);
                    }
                    catch {; }
                }
            }
            return true;
        }
    }
}
