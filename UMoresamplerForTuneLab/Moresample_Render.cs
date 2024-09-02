using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab;
using UtaubaseForTuneLab.Utils;

namespace UMoresamplerForTuneLab
{
    internal class Moresample_Render(string enginePath) : IRenderEngine
    {
        public string ResamplerPath { get { return Path.Combine(enginePath,"enginebin","moresampler.exe"); } }
        public string WavtoolPath { get {return Path.Combine(enginePath, "enginebin", "moresampler.exe");} }

        public string EngineUniqueString => "UMoresampler";

        public bool WindowsOnly => true;





        public const string OpeningID = "MOpening";
        public readonly static NumberConfig OpeningConfig = new() { DefaultValue = 0, MinValue = -1, MaxValue = 1 };
        public const string PressingID = "UCompressing";
        public readonly static NumberConfig CompressConfig = new() { DefaultValue = 0.86, MinValue = 0, MaxValue = 1 };
        public const string ConsonantBreathID = "UConsonantBreath";
        public readonly static NumberConfig ConsonantBreathConfig = new() { DefaultValue = 0, MinValue = -0.2, MaxValue = 1 };
        public const string StretchModeID = "MStretchMode";
        public readonly static EnumConfig StretchModeConfig = new EnumConfig(new List<string>(["Auto","VowelStretch","TimeLoop"]), 0);


        public const string XOpeningID = "XTrack:MOpening Corrected";
        public readonly static NumberConfig XOpeningConfig = new() { DefaultValue = 0, MinValue = -1, MaxValue = 1 };
        public const string XGenderID = "XTrack:UGender Corrected";
        public readonly static NumberConfig XGenderConfig = new() { DefaultValue = 0, MinValue = -0.5, MaxValue = 0.5 };
        public const string XBreathnessID = "XTrack:MBreathness Corrected";
        public readonly static NumberConfig XBreathnessConfig = new() { DefaultValue = 0, MinValue = -1, MaxValue = 1 };
        public const string XTenseID = "XTrack:MTense Corrected";
        public readonly static NumberConfig XTenseConfig = new() { DefaultValue = 0, MinValue = -1, MaxValue = 1 };

        public const string GenderID = "UGender";
        public readonly static AutomationConfig GenderConfig = new("GEN", 0, -0.5, 0.5, "#86E573");
        public const string TenseID = "MTense";
        public readonly static AutomationConfig TenseConfig = new("TEN", 0, -1, 1, "#86E573");
        public const string ResonID = "MReson";
        public readonly static AutomationConfig ResonConfig = new("RES", 0, -1, 1, "#86E573");
        public const string BreathnessID = "MBreathness";
        public readonly static AutomationConfig BreathnessConfig = new("BRE", 0, -1, 1, "#86E573");
        public const string DryID = "MDryness";
        public readonly static AutomationConfig DryConfig = new("DRY", 0, -1, 1, "#86E573");
        public const string GrowlID = "MGrowl";
        public readonly static AutomationConfig GrowlConfig = new("GWL", 0, 0, 1, "#86E573");
        public const string CrudeID = "MCrude";
        public readonly static AutomationConfig CrudeConfig = new("CUE", 0, 0, 1, "#86E573");
        public const string DistortID = "MDistort";
        public readonly static AutomationConfig DistortConfig = new("DSR", 0, 0, 1, "#86E573");
        public const string EnhanceID = "MEnhance";
        public readonly static AutomationConfig EnhanceConfig = new("MEH", 0, 0, 1, "#86E573");

        public IReadOnlyOrderedMap<string, AutomationConfig> AutomationConfigs { get; set; } = new OrderedMap<string, AutomationConfig>()
        {
            {BreathnessID,BreathnessConfig },
            {GrowlID,GrowlConfig },
            {GenderID,GenderConfig },
            {TenseID,TenseConfig },
            {ResonID,ResonConfig },
            {DryID,DryConfig },
            {CrudeID, CrudeConfig },
            {DistortID, DistortConfig },
            {EnhanceID, EnhanceConfig }
        };

        public IReadOnlyOrderedMap<string, IPropertyConfig> PartProperties { get; set; } = new OrderedMap<string, IPropertyConfig>()
        {
            {PressingID,CompressConfig },
            {StretchModeID,StretchModeConfig}
        };

        public IReadOnlyOrderedMap<string, IPropertyConfig> NoteProperties { get; set; } = new OrderedMap<string, IPropertyConfig>()
        {
            {OpeningID,OpeningConfig },
            {ConsonantBreathID,ConsonantBreathConfig },

            {XBreathnessID,XBreathnessConfig },
            {XGenderID,XGenderConfig },
            {XOpeningID,XOpeningConfig},
            {XTenseID,XTenseConfig }
        };

        public string GetNoteFlags(ISynthesisData data, ISynthesisNote note, string baseFlag = "", bool isXTrackFlag = false)
        {
            Moresample_Flags flags = new Moresample_Flags();
            if (baseFlag != "") flags.Parse(baseFlag);
            void SetFlagType1(string key, string ID, double defVal = 0, int minV = -100, int maxV = 100, bool isXTrackFlag=false, string XID="")
            {
                var tmp = note.Properties.GetDouble(ID, defVal);
                if(isXTrackFlag && XID.Length>0)
                {
                    tmp+=note.Properties.GetDouble(XID, 0);
                }
                var val = MathUtils.RoundLimit(tmp * 100.0, minV, maxV);
                if (tmp != defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = val; else flags.FlagDict.Add(key, val); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key);
            }
            void SetFlagType2(string key, string ID, double defVal = 0, int minV = -100, int maxV = 100, bool isXTrackFlag = false, string XID = "")
            {
                var tmp = data.PartProperties.GetDouble(ID, defVal);
                if (isXTrackFlag && XID.Length > 0)
                {
                    tmp += note.Properties.GetDouble(XID, 0);
                }
                var val = MathUtils.RoundLimit(tmp * 100.0, minV, maxV);
                if (tmp != defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = val; else flags.FlagDict.Add(key, val); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key);
            }

            //Opening
            SetFlagType1("Mo", OpeningID, 0, -100, 100,isXTrackFlag,XOpeningID);
            //b flag
            SetFlagType1("b", ConsonantBreathID, 0, -20, 100);
            //Pressing
            SetFlagType2("P", PressingID, 0.86, 0, 100);
            //Stretch
            {
                string tmp = data.PartProperties.GetString(StretchModeID, "Auto");
                if (tmp.ToLower() == "timeloop")
                {
                    if (flags.FlagDict.ContainsKey("e")) flags.FlagDict.Remove("e");
                    if (flags.FlagDict.ContainsKey("Me")) flags.FlagDict["Me"] = 1;
                    else flags.FlagDict.Add("Me", 1);
                }
                else if (tmp.ToLower() == "vowelstretch")
                {
                    if (flags.FlagDict.ContainsKey("Me")) flags.FlagDict.Remove("Me");
                    if (flags.FlagDict.ContainsKey("e")) flags.FlagDict["e"] = 1;
                    else flags.FlagDict.Add("e", 1);
                }
                else
                {
                    if (flags.FlagDict.ContainsKey("e")) flags.FlagDict.Remove("e");
                    if (flags.FlagDict.ContainsKey("Me")) flags.FlagDict.Remove("Me");
                }
            }

            return flags.ToString();
        }

        public string GetTimeFlags(ISynthesisData data, double time, string baseFlag = "", double areaDuration = 0, bool isXTrackFlag=false)
        {
            void SetProperyType1(string key, string ID, ref Moresample_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
            {
                AutoPropertyGetter getter = new AutoPropertyGetter(data, ID,-1,1, defVal);
                var vNote = new AutoPropertyGetter.VirtualNote(time, time + areaDuration);
                double value = getter.GetNoteBarValue(vNote, AutoPropertyGetter.PropertyType.AttrackBar, AutoPropertyGetter.ValueSelectType.FarZero);
                if (isXTrackFlag && XID.Length > 0)try{
                    value += data.Notes.Where(p=>p.StartTime<=time && p.EndTime>=time+areaDuration).First().Properties.GetDouble(XID, 0);
                    }
                    catch {; }
                int tValue = MathUtils.RoundLimit(value * 100.0, -100, 100);
                if (value == defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = tValue;
                else flags.FlagDict.Add(key, tValue);
            }
            void SetProperyType2(string key, string ID, ref Moresample_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
            {
                AutoPropertyGetter getter = new AutoPropertyGetter(data, ID,0,1, defVal);
                var vNote = new AutoPropertyGetter.VirtualNote(time, time + areaDuration);
                double value = getter.GetNoteBarValue(vNote, AutoPropertyGetter.PropertyType.AttrackBar, AutoPropertyGetter.ValueSelectType.FarZero);
                if (isXTrackFlag && XID.Length > 0) try
                    {
                        value += data.Notes.Where(p => p.StartTime <= vNote.StartTime && p.EndTime >= vNote.EndTime).First().Properties.GetDouble(XID, 0);
                    }
                    catch {; }
                int tValue = MathUtils.RoundLimit(value * 100.0, 0, 100);
                if (value == defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = tValue;
                else flags.FlagDict.Add(key, tValue);
            }

            Moresample_Flags flags = new Moresample_Flags();
            if (baseFlag != "") flags.Parse(baseFlag);
            //Gender
            SetProperyType1("g", GenderID, ref flags,0,isXTrackFlag,XGenderID);
            //Teson
            SetProperyType1("Mt", TenseID, ref flags,0,isXTrackFlag,XTenseID);
            //Breath
            SetProperyType1("Mb", BreathnessID, ref flags,0,isXTrackFlag,XBreathnessID);
            //Reson
            SetProperyType1("Mr", ResonID, ref flags);
            //Dry
            SetProperyType1("Md", DryID, ref flags);
            //GROWL
            SetProperyType2("MG", GrowlID, ref flags);
            //CUE
            SetProperyType2("MC", CrudeID, ref flags);
            //Distort
            SetProperyType2("MD", DistortID, ref flags);
            //ENHANCE
            SetProperyType1("ME", EnhanceID, ref flags);
            return flags.ToString();
        }
    }
}
