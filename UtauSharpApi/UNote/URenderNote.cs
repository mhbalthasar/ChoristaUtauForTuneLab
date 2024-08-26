using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UtauSharpApi.Utils;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UNote
{
    public class USTPFix(URenderNote pNote)
    {
        public double FixPreuttr { get; set; } = 0;
        public double FixOverlap { get; set; } = 0;
        public double FixSTPoint { get; set; } = 0;

        public void UpdateSTPFix()
        {
            if (pNote == null || pNote.IsRest)
            {
                FixPreuttr = 0; FixOverlap = 0; FixSTPoint = 0;
                return;
            }

            double correctRate = 1.0;
            double velocityRate = Math.Pow(2, 1 - pNote.Velocity / 100.0);
            double curPreuttr = pNote.RenderOto.Preutter * velocityRate;
            double curOverlap = pNote.RenderOto.Overlap * velocityRate;

            if (pNote != null && pNote.PrevNote != null)
            {
                double prevDuration = pNote.PrevNote.DurationMSec;
                double maxOccupy = pNote.PrevNote.IsRest ? prevDuration : prevDuration / 2;
                if (curPreuttr - curOverlap > maxOccupy)
                {
                    correctRate = maxOccupy / (curPreuttr - curOverlap);
                }
            }

            double correctPreuttr = correctRate * curPreuttr;
            double correctOverlap = correctRate * curOverlap;
            FixSTPoint = curPreuttr - correctPreuttr;
            FixPreuttr = correctPreuttr - pNote.RenderOto.Preutter;
            FixOverlap = correctOverlap - pNote.RenderOto.Overlap;
        }
    }

    public class URenderNote
    {
        public URenderNote() { FixSTP = new USTPFix(this); }
        public URenderNote? PrevNote { get; set; } = null;
        public URenderNote? NextNote { get; set; } = null;
        public Oto? RenderOto { get; set; } = null;
        public bool IsRest { get => RenderOto == null || RenderOto.Alias == "R"; }
        public USTPFix FixSTP { get; private set; }
        public string VoiceBankPath { get; set; } = "";
        public double StartMSec { get; set; } = 0;
        public double DurationMSec { get; set; } = 0;
        public int NoteNumber { get; set; } = 60;
        public string Flags { get; set; } = "";
        public string EngineSalt { get; set; } = "";
        public int Velocity { get; set; } = 100;
        public double STPoint { get; set; } = 0;
        public List<int> PitchBends { get; set; } = new List<int>();
        public double GainVolDiff { get; set; } = 0;


        //for resampler
        #region
        private string NoteString()
        {
            return OctaveUtils.NoteNumber2Str(NoteNumber - 12);
        }
        private double PitchStartMSec()
        {
            double fixedStp = IsRest ? 0 : STPoint + FixSTP.FixSTPoint;
            double fixedPreuttr = IsRest ? 0 : RenderOto.Preutter + FixSTP.FixPreuttr;
            return StartMSec - fixedPreuttr - fixedStp;
        }
        private double PitchDurationMSec()
        {
            double RealLength = DurationMSec + FixedDurationDiff();
            return RealLength;
        }
        private string PitchLines = "";
        public void SetPitchBends(Func<double[], double[]> PitchGetter)
        {
            if (IsRest) return;
            List<double> millsec_times = new List<double>();
            double t = PitchStartMSec();// - 25;
            double end = t + PitchDurationMSec();
            while (t < end) { millsec_times.Add(t); t += 5.0; }
            var pitcharray = PitchGetter(millsec_times.ToArray());
            //PitToStr
            List<string> encodedPit = new List<string>();
            const string Base64EncodeMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            string lep = "";
            int cep = 0;
            foreach (var pit in pitcharray)
            {
                int pcent = (int)Math.Round((pit - NoteNumber) * 100.0);
                pcent = Math.Max(Math.Min(pcent, 2047), -2048);
                pcent = pcent >= 0 ? pcent : pcent + 4096;
                char x = Base64EncodeMap[(int)(pcent / 64.0)];
                char y = Base64EncodeMap[pcent % 64];
                string ep = "" + x + y;
                if (ep == lep)
                {
                    cep = cep + 1;
                }
                else
                {
                    if (lep.Length > 0 && cep > 0)
                    {
                        if (cep > 2)
                        {
                            encodedPit.Add(lep + "#" + (cep - 1).ToString() + "#");
                        }
                        else if (cep == 2) { encodedPit.Add(lep); encodedPit.Add(lep); }
                        else { encodedPit.Add(lep); }
                        lep = ""; cep = 0;
                    }
                    lep = ep;
                    cep = 1;
                }
            }
            if (lep.Length > 0 && cep > 0)
            {
                if (cep > 2)
                {
                    encodedPit.Add(lep + "#" + (cep - 1).ToString() + "#");
                }
                else if (cep == 2) { encodedPit.Add(lep); encodedPit.Add(lep); }
                else { encodedPit.Add(lep); }
                lep = ""; cep = 0;
            }
            PitchLines = string.Join("", encodedPit);
        }
        private List<string> GetSampleArgs()
        {
            double RealLength = DurationMSec + FixedDurationDiff();
            List<string> ret = new List<string>();
            ret.Add(NoteString());
            ret.Add(Velocity.ToString());
            ret.Add(Flags);
            //oto
            ret.Add(RenderOto.Offset.ToString("F3"));
            ret.Add(RealLength.ToString("F3"));
            ret.Add(RenderOto.Consonant.ToString("F3"));
            ret.Add(RenderOto.Cutoff.ToString("F3"));
            ret.Add("100");
            ret.Add("0");
            ret.Add("!125");//5ms space each point
            ret.Add(PitchLines);
            return ret;
        }
        public List<string> GetResamplerArgs(bool ToolIsWindowsPlatform = true)
        {
            if (IsRest) return new List<string>();
            List<string> ret = new List<string>();
            ret.Add(ToolIsWindowsPlatform ? CrossPlatformUtils.KeepWindows(RenderOto.GetWavfilePath(VoiceBankPath)) : RenderOto.GetWavfilePath(VoiceBankPath));
            ret.Add(ToolIsWindowsPlatform ? CrossPlatformUtils.KeepWindows(TempFilePath) : TempFilePath);
            ret.AddRange(GetSampleArgs());
            return ret;
        }
        #endregion

        //for wavtool
        #region
        private double FadeInMSec
        {
            get
            {
               // if (PrevNote == null || PrevNote.RenderOto==null) return 5;
                if (IsRest) return 5;
                return Math.Max(5, RenderOto.Overlap+FixSTP.FixOverlap);// PrevNote.RenderOto.Overlap + PrevNote.FixSTP.FixOverlap);
            }
        }
        private double FadeOutMSec
        {
            get
            {
                if (NextNote == null || NextNote.RenderOto==null) return 35;
                if (IsRest) return 35;
                return Math.Max(35, NextNote.RenderOto.Overlap + NextNote.FixSTP.FixOverlap);
            }
        }
        //WavToolArg4
        private string WavAppendLengthStr()
        {
            double fix = FixedDurationDiff();
            string ret = string.Format("{0}@125", Math.Round(DurationMSec));//125bpm，1tick=1ms
            ret = ret + (fix >= 0 ? "+" : "") + fix.ToString("F3");
            return ret;
        }
        //WavToolArg5
        private double[] Envlope()
        {

            //0     5       55.1   0  100  100  0 17.415
            //p1    p2      p3     v1  v2  v3  v4   ovr
            //0   prevOvl  nextOvl 0  100  100 0  thisOvl
            if (IsRest) return [0, 0];
            var ret = new double[8];
            ret[0] = 0;//p1
            ret[1] = FadeInMSec;//p2
            ret[2] = FadeOutMSec;//p3
            ret[3] = 0;//v1
            ret[4] = GainVolDiff * 100 + 100.0;//v2
            ret[5] = GainVolDiff * 100 + 100.0;//v3
            ret[6] = 0;//v4
            ret[7] = RenderOto.Overlap + FixSTP.FixOverlap;
            return ret;
        }
        public List<string> GetWavToolArgs(string targetWavPath = "temp.wav", bool ToolIsWindowsPlatform = true)
        {
            List<string> ret = new List<string>();
            ret.Add(ToolIsWindowsPlatform ? CrossPlatformUtils.KeepWindows(targetWavPath) : targetWavPath);
            ret.Add(ToolIsWindowsPlatform ? CrossPlatformUtils.KeepWindows(TempFilePath) : TempFilePath);
            ret.Add((STPoint + FixSTP.FixSTPoint).ToString("F3"));
            ret.Add(WavAppendLengthStr());
            ret.AddRange(Envlope().Select(d => d.ToString("F3")));
            return ret;
        }
        #endregion

        //BatchBat FileContent
        #region
        private List<string> BatchBat_GetParams()
        {
            List<string> ret = new List<string>();
            ret.Add("100");
            ret.Add("0");
            ret.Add("!125");//5ms space each point
            ret.Add(PitchLines);
            return ret;
        }
        private List<string> BatchBat_GetEnv()
        {
            return Envlope().Select(d => d.ToString("F3")).ToList();
        }
        private string BatchBat_GetVel()
        {
            return Velocity.ToString();
        }
        private string BatchBat_GetTemp()
        {
            return string.Format("\"{0}\"", TempFilePath);
        }
        private List<string> BatchBat_GetHelperArgs()
        {
            double RealLength = DurationMSec + FixedDurationDiff();
            List<string> ret = new List<string>();
            ret.Add(string.Format("\"{0}\"", CrossPlatformUtils.KeepWindows(RenderOto.GetWavfilePath(VoiceBankPath))));//%1
            ret.Add(NoteString());//%2
            ret.Add(WavAppendLengthStr());//%3
            ret.Add((RenderOto.Preutter + FixSTP.FixPreuttr).ToString("F3"));//%4?没用？
            ret.Add(RenderOto.Offset.ToString("F3"));//%5
            ret.Add(RealLength.ToString("F3"));//%6
            ret.Add(RenderOto.Consonant.ToString("F3"));//%7
            ret.Add(RenderOto.Cutoff.ToString("F3"));//%8
            ret.Add("1");//%9也没用
            return ret;
        }
        public string GetBatchBat()
        {
            if (IsRest)
            {
                List<string> args = new List<string>();
                args.Add(string.Format("\"{0}\"", CrossPlatformUtils.KeepWindows(Path.Combine(VoiceBankPath, "R.wav"))));
                args.Add("0");
                args.Add(WavAppendLengthStr());
                args.Add("0");
                args.Add("0");
                return "@\"%tool%\" \"%output%\" " + string.Join(" ", args);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@set params=" + string.Join(" ", BatchBat_GetParams()));
            sb.AppendLine("@set env=" + string.Join(" ", BatchBat_GetEnv()));
            sb.AppendLine("@set vel=" + BatchBat_GetVel());
            sb.AppendLine("@set temp=" + BatchBat_GetTemp());
            sb.AppendLine("@set flag=\"" + Flags + "\"");
            //sb.AppendLine("@set stp="+(STPoint+FixSTP.FixSTPoint).ToString("F3"));
            sb.AppendLine("@echo Processing");
            sb.AppendLine("@call %helper% " + string.Join(" ", BatchBat_GetHelperArgs()));
            return sb.ToString();
        }
        #endregion

        //Temp File NameGenerate && GlobalFixed
        #region
        private double FixedDurationDiff()
        {
            //末端偏移
            double tailFix = NextNote == null || NextNote.IsRest ? 0 :
                    NextNote.RenderOto.Overlap + NextNote.FixSTP.FixOverlap - (NextNote.RenderOto.Preutter + NextNote.FixSTP.FixPreuttr)
                ;
            tailFix = NextNote == null ? tailFix : Math.Min(tailFix, NextNote.DurationMSec);
            //先行发音
            double fix = (IsRest ? 0 : RenderOto.Preutter) + FixSTP.FixPreuttr + tailFix;

            return fix;
        }

        private static string GetMixedHash(List<string> hashes, string appendSalt = "")
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(appendSalt + string.Join("\r\n", hashes));
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public string TempFilePath
        {
            get
            {
                if (IsRest) return Path.Combine(VoiceBankPath, "R.wav");
                string resWav = RenderOto.GetWavfilePath(VoiceBankPath);
                string hash = GetMixedHash(GetSampleArgs(), EngineSalt + "\r\n" + resWav + "\r\n");
                string tmpPath = Path.Combine(Path.GetTempPath(), "UtauSharp", "ResamplerCache");
                if (!Directory.Exists(tmpPath)) { Directory.CreateDirectory(tmpPath); }
                return Path.Combine(tmpPath, string.Format("{0}.wav", hash));
            }
        }//TODO
        #endregion
    }
}
