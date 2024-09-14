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

namespace UDoppeltlerForTuneLab
{
    internal class Doppeltler_Render(string enginePath) : IRenderEngine
    {
        public override string ResamplerPath { get { return Path.Combine(enginePath,"enginebin","doppeltler32", "doppeltler32.exe"); } }
        public override string WavtoolPath { get {return Path.Combine(enginePath, "enginebin", "doppeltler32", "wavtool32.exe");} }

        public override string EngineUniqueString => "ChoristaUtau_Doppe";
        public override int GetQueueSize(int defaultQueueSize)
        {
            return Math.Max(1, defaultQueueSize);
        }


        public const string ToneShiftID = "Flags:ToneShift";
        public readonly static NumberConfig ToneShiftConfig = new() { DefaultValue = 0, MinValue = -12, MaxValue = +12 };
        public const string PeakcompressorID = "Flags:Peakcompressor";
        public readonly static NumberConfig CompressConfig = new() { DefaultValue = 0.86, MinValue = 0, MaxValue = 1 };
        public const string ConsonantBreathID = "Flags:breath";
        public readonly static NumberConfig ConsonantBreathConfig = new() { DefaultValue = 0, MinValue = -0.2, MaxValue = 1 };

        public const string XGenderID = "XTrack:gender Corrected";
        public readonly static NumberConfig XGenderConfig = new() { DefaultValue = 0, MinValue = -0.5, MaxValue = 0.5 };
        public const string GenderID = "Flags:gender";
        public readonly static AutomationConfig GenderConfig = new("g", 0, -0.5, 0.5, "#86E573");

        public const string TLIID = "Flags(i):FixedTailLength";
        public readonly static AutomationConfig TLIConfig = new("TLI", 0, 0, 200, "#86E573");
        public const string TLVID = "Flags(v):FixedTailVelocity";
        public readonly static AutomationConfig TLVConfig = new("TLV", 0, 0, 2, "#86E573");


        public override IReadOnlyOrderedMap<string, AutomationConfig> AutomationConfigs { get; } = new OrderedMap<string, AutomationConfig>()
        {
            {GenderID,GenderConfig },
            {TLIID,TLIConfig }, {TLVID, TLVConfig}
        };

        public override IReadOnlyOrderedMap<string, IPropertyConfig> PartProperties { get; } = new OrderedMap<string, IPropertyConfig>()
        {
            {PeakcompressorID,CompressConfig },
        };

        public override IReadOnlyOrderedMap<string, IPropertyConfig> NoteProperties { get; } = new OrderedMap<string, IPropertyConfig>()
        {
            {ConsonantBreathID,ConsonantBreathConfig },
            {ToneShiftID,ToneShiftConfig },
            {XGenderID,XGenderConfig }
        };

        public override string GetNoteFlags(ISynthesisData data, ISynthesisNote note, string baseFlag = "", bool isXTrackFlag = false)
        {
            Doppeltler_Flags flags = new Doppeltler_Flags();
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

            //b flag
            SetFlagType1("B", ConsonantBreathID, 0, -20, 100);
            //Pressing
            SetFlagType2("P", PeakcompressorID, 0.86, 0, 100);
            //t flag
            SetFlagType1("t", ToneShiftID, 0, -1200, 1200);

            return flags.ToString();
        }

        public override string GetTimeFlags(ISynthesisData data, double time, string baseFlag = "", double areaDuration = 0, bool isXTrackFlag=false)
        {
            void SetProperyType1(string key, string ID, ref Doppeltler_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
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
            void SetProperyType2(string key, string ID, ref Doppeltler_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
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
            void SetProperyType3(string key, string ID, ref Doppeltler_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
            {
                AutoPropertyGetter getter = new AutoPropertyGetter(data, ID, -1, 1, defVal);
                var vNote = new AutoPropertyGetter.VirtualNote(time, time + areaDuration);
                double value = getter.GetNoteBarValue(vNote, AutoPropertyGetter.PropertyType.ReleaseBar, AutoPropertyGetter.ValueSelectType.FarZero);
                if (isXTrackFlag && XID.Length > 0) try
                    {
                        value += data.Notes.Where(p => p.StartTime <= time && p.EndTime >= time + areaDuration).First().Properties.GetDouble(XID, 0);
                    }
                    catch {; }
                int tValue = MathUtils.RoundLimit(value * 100.0, 0, 200);
                if (value == defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = tValue;
                else flags.FlagDict.Add(key, tValue);
            }
            void SetProperyType4(string key, string ID, ref Doppeltler_Flags flags, int defVal = 0, bool isXTrackFlag = false, string XID = "")
            {
                AutoPropertyGetter getter = new AutoPropertyGetter(data, ID, -1, 1, defVal);
                var vNote = new AutoPropertyGetter.VirtualNote(time, time + areaDuration);
                double value = getter.GetNoteBarValue(vNote, AutoPropertyGetter.PropertyType.ReleaseBar, AutoPropertyGetter.ValueSelectType.FarZero);
                if (isXTrackFlag && XID.Length > 0) try
                    {
                        value += data.Notes.Where(p => p.StartTime <= time && p.EndTime >= time + areaDuration).First().Properties.GetDouble(XID, 0);
                    }
                    catch {; }
                int tValue = MathUtils.RoundLimit(value, 0, 200);
                if (value == defVal) { if (flags.FlagDict.ContainsKey(key)) flags.FlagDict.Remove(key); }
                else if (flags.FlagDict.ContainsKey(key)) flags.FlagDict[key] = tValue;
                else flags.FlagDict.Add(key, tValue);
            }

            Doppeltler_Flags flags = new Doppeltler_Flags();
            if (baseFlag != "") flags.Parse(baseFlag);
            //Gender
            SetProperyType1("g", GenderID, ref flags,0,isXTrackFlag,XGenderID);
            //Gender
            SetProperyType4("i", TLIID, ref flags, 0);
            //Gender
            SetProperyType3("v", TLVID, ref flags, 0);

            return flags.ToString();
        }
    }
}
