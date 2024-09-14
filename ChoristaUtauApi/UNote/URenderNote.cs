using System.Text;
using System.Xml.Linq;
using ChoristaUtauApi.Utils;
using ChoristaUtauApi.UVoiceBank;

namespace ChoristaUtauApi.UNote
{
    public class URenderNote_Attributes(URenderNote pNote)
    {
        public bool IsRest { get => pNote == null || pNote.RenderOto == null || pNote.RenderOto.Alias == "R"; }
        public string InputWavFile { get; set; } = "";
        public string Tone { get => OctaveUtils.NoteNumber2Str(pNote.NoteNumber - 12); }
        public string Flags { get => pNote.Flags; }
        public int Velocity { get => pNote.Velocity; }
        public int Modulation { get; private set; } = 0;
        public int StaticTempo { get; private set; } = 125;
        public double Preutter { get; set; } = 0;
        public double Overlap { get; set; } = 0;
        public double Offset { get; set; } = 0;
        public double SkipOver { get; set; } = 0;//STPoint
        public double Consonant { get; set; } = 0;
        public double Cutoff { get; set; } = 0;
        public double DurCorrection { get; set; } = 0;
        public double DurRequired { get; set; } = 0;
        public double TailIntrude { get; set; } = 0;
        public double TailOverlap { get; set; } = 0;
        public double TailOffset { get; set; } = 0;
        public double EnvFadeIn { get; set; } = 0;
        public double EnvFadeOut { get => TailOverlap; }
        public string PitchLineString { get; set; } = "";
        public bool UltraPitchLine { get; set; } = false;
        private void GenerateFixedSTPs()
        {
            double OtoPreutter, OtoOverlap;
            {//参数修正
                //获取新Offset，偏移结果不能为负值
                var nOtoOffset = (pNote.RenderOto.Offset + pNote.OtoOffsetCorrected) < 0 ? -pNote.RenderOto.Offset : pNote.OtoOffsetCorrected;

                //获取新PU
                var nOtoPreutter = pNote.RenderOto.Preutter + pNote.OtoPreutterCorrected + nOtoOffset;
                //PU不能为负值
                OtoPreutter = (pNote.RenderOto.Offset + nOtoPreutter) < 0 ? -pNote.RenderOto.Offset : nOtoPreutter;

                //获取新OP
                var nOtoOverlap = pNote.RenderOto.Overlap + pNote.OtoOverlapCorrected + nOtoOffset;
                //OP不能为负值
                OtoOverlap = (pNote.RenderOto.Offset + nOtoOverlap) < 0 ? -pNote.RenderOto.Offset : nOtoOverlap;
            }

            var consonantStretchRatio = Math.Pow(2, 1.0 - Velocity * 0.01);
            var autoOverlap = OtoOverlap * consonantStretchRatio;
            var autoPreutter = OtoPreutter * consonantStretchRatio;
            var origPreutter = OtoPreutter * consonantStretchRatio;
            bool overlapped = false;
            TailIntrude = 0;
            TailOverlap = 0;
            var Prev = pNote.PrevNote;
            if (Prev != null)
            {
                double gapMs = pNote.StartMSec - Prev.EndMSec;
                double prevDur = Prev.DurationMSec;
                double maxPreutter = autoPreutter;
                if (gapMs <= 0)
                {
                    overlapped = true;
                    if (autoPreutter - autoOverlap > prevDur * 0.5f)
                    {
                        maxPreutter = prevDur * 0.5f / (autoPreutter - autoOverlap) * autoPreutter;
                    }
                }
                else if (gapMs < autoPreutter)
                {
                    maxPreutter = gapMs;
                }
                if (autoPreutter > maxPreutter)
                {
                    double ratio = maxPreutter / autoPreutter;
                    autoPreutter = maxPreutter;
                    autoOverlap *= ratio;
                }
                if (autoPreutter > prevDur * 0.9f && overlapped)
                {
                    double delta = autoPreutter - prevDur * 0.9f;
                    autoPreutter -= delta;
                    autoOverlap -= delta;
                }
            }
            Preutter = Math.Max(0, autoPreutter);
            Overlap = autoOverlap;
            SkipOver = origPreutter - Preutter;
            EnvFadeIn = overlapped ? Math.Max(Preutter, Preutter - Overlap) : 5;
            if (Prev != null)
            {
                Prev.Attributes.TailIntrude = overlapped ? Math.Max(Preutter, Preutter - Overlap) : 0;
                Prev.Attributes.TailOverlap = overlapped ? Math.Max(Overlap, 0) : 0;
                Prev.Attributes.TailOffset = overlapped ? Math.Max(0, Preutter - Overlap) : 0;
            }
        }
        public void UpdateAttributes(
            Func<URenderNote, URenderNote>? CallbackBeforeUpdates = null,
            Func<URenderNote, URenderNote>? CallbackAfterUpdates = null
            )
        {
            if (pNote == null) return;
            if (CallbackBeforeUpdates != null)
            {
                var nNote = CallbackBeforeUpdates(pNote);
                if (nNote != null)
                {
                    if (pNote.OtoPreutterCorrected != nNote.OtoPreutterCorrected) pNote.OtoPreutterCorrected = nNote.OtoPreutterCorrected;
                    if (pNote.OtoOverlapCorrected != nNote.OtoOverlapCorrected) pNote.OtoOverlapCorrected = nNote.OtoOverlapCorrected;

                    if (pNote.OtoConsonantCorrected != nNote.OtoConsonantCorrected) pNote.OtoConsonantCorrected = nNote.OtoConsonantCorrected;
                    if (pNote.OtoOffsetCorrected != nNote.OtoOffsetCorrected) pNote.OtoOffsetCorrected = nNote.OtoOffsetCorrected;
                    if (pNote.OtoCutoffCorrected != nNote.OtoCutoffCorrected) pNote.OtoCutoffCorrected = nNote.OtoCutoffCorrected;

                    if (pNote.Flags != nNote.Flags) pNote.Flags = nNote.Flags;
                }
            }
            if (pNote.RenderOto != null)
            {
                InputWavFile = pNote.RenderOto.GetWavfilePath(pNote.VoiceBankPath);
                GenerateFixedSTPs();
                {

                    //获取新Offset，偏移结果不能为负值
                    var nOtoOffset = (pNote.RenderOto.Offset + pNote.OtoOffsetCorrected) < 0 ? -pNote.RenderOto.Offset : pNote.OtoOffsetCorrected;
                    //设置Offset
                    Offset = pNote.RenderOto.Offset + nOtoOffset;

                    //获取新FixedLength
                    var nOtoConsonant = pNote.RenderOto.Consonant + pNote.OtoConsonantCorrected + nOtoOffset;
                    Consonant = (pNote.RenderOto.Offset + nOtoConsonant) < 0 ? 0 : nOtoConsonant;

                    //设置Cutoff
                    if(pNote.RenderOto.Cutoff < 0)
                    {
                        //LeftBaner
                        var nOtoCutoffType1 = (-pNote.RenderOto.Cutoff) + pNote.OtoConsonantCorrected + nOtoOffset;
                        var CutoffType1 = (pNote.RenderOto.Offset + nOtoCutoffType1) < 0 ? 0 : nOtoCutoffType1;
                        Cutoff = -CutoffType1;
                    }
                    else
                    {
                        //RightBaner
                        var nOtoCutoffType2 = pNote.RenderOto.Cutoff - pNote.OtoCutoffCorrected;
                        var CutoffType2 = nOtoCutoffType2 < 0 ? 0 : nOtoCutoffType2;
                        Cutoff = CutoffType2;
                    }
                }
                DurCorrection = Preutter - TailIntrude + TailOverlap;
                {
                    DurRequired = pNote.DurationMSec + DurCorrection + SkipOver;
                    DurRequired = Math.Max(DurRequired, pNote.RenderOto.Consonant);
                    DurRequired = Math.Ceiling(DurRequired / 50.0 + 0.5) * 50.0;
                }
            }
            if (CallbackAfterUpdates != null)
            { 
                var nNote = CallbackAfterUpdates(pNote);
                if (nNote != null)
                {
                    pNote.AttrackVolume = nNote.AttrackVolume;
                    pNote.ReleaseVolume = nNote.ReleaseVolume;
                    if (pNote.Flags != nNote.Flags) pNote.Flags = nNote.Flags;
                }
            }
        }


        public void SetPitchLine(Func<double[], double[]> PitchGetter, bool ultraMode = false)
        {
            if (IsRest) return;
            List<double> millsec_times = new List<double>();
            double t = (pNote.StartMSec - EnvFadeIn - SkipOver);
            double end = DurRequired + t;
            UltraPitchLine = ultraMode;
            while (t < end) { millsec_times.Add(t); t += ultraMode ? 1.0: 5.0; }
            var pitcharray = PitchGetter(millsec_times.ToArray());
            //PitToStr
            List<string> encodedPit = new List<string>();
            const string Base64EncodeMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            string lep = "";
            int cep = 0;
            foreach (var pit in pitcharray)
            {
                int pcent = (int)Math.Round((pit - pNote.NoteNumber) * 100.0);
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
            PitchLineString = string.Join("", encodedPit);
        }
    }
    public class URenderNote_RenderExecutor(URenderNote pNote)
    {
        public string TempFilePath { get; private set; } = "";
        private List<string> Attr_EnvlopeArgs { get; set; } = new List<string>();
        private string Attr_FixedDuration { get; set; } = "";
        private List<string> Attr_SynthesisArgs { get; set; } = new List<string>();
        private void UpdateTempFileName()
        {
            if (pNote.Attributes.IsRest) TempFilePath = Path.Combine(pNote.VoiceBankPath, "R.wav");
            List<string> HashArgs = new List<string>();
            HashArgs.Add(pNote.EngineSalt);//Add Engine Signal
            HashArgs.Add(pNote.Attributes.InputWavFile);
            HashArgs.AddRange(Attr_SynthesisArgs);

            string hash = GetMixedHash(HashArgs);
            string tmpPath = Path.Combine(Path.GetTempPath(), "ChoristaUtau", "ResamplerCache");
            if (!Directory.Exists(tmpPath)) { Directory.CreateDirectory(tmpPath); }

            TempFilePath = Path.Combine(tmpPath, string.Format("{0}.wav", hash));
        }
        private string GetMixedHash(List<string> hashes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(string.Join("\r\n", hashes));
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        private string CPath(string path, bool isWindowsTool = true)
        {
            return isWindowsTool ? CrossPlatformUtils.KeepWindows(path) : path;
        }
        private void UpdateSynthesisArgs()
        {
            Attr_SynthesisArgs.Clear();
            if (pNote.Attributes.IsRest) { return; }
            Attr_SynthesisArgs.Add(pNote.Attributes.Tone);
            Attr_SynthesisArgs.Add(pNote.Attributes.Velocity.ToString());
            Attr_SynthesisArgs.Add("\""+pNote.Attributes.Flags+"\"");
            Attr_SynthesisArgs.Add(pNote.Attributes.Offset.ToString("F3"));
            Attr_SynthesisArgs.Add((pNote.Attributes.DurRequired - pNote.Attributes.TailOffset).ToString("F3"));
            Attr_SynthesisArgs.Add(pNote.Attributes.Consonant.ToString("F3"));
            Attr_SynthesisArgs.Add(pNote.Attributes.Cutoff.ToString("F3"));
            Attr_SynthesisArgs.Add("100");//Volume
            Attr_SynthesisArgs.Add(pNote.Attributes.Modulation.ToString("F3"));
            Attr_SynthesisArgs.Add(pNote.Attributes.UltraPitchLine?"!625":"!125");//Tempo
            Attr_SynthesisArgs.Add(pNote.Attributes.PitchLineString);
        }
        private void UpdateEnvlope()
        {
            if (pNote.Attributes.IsRest) { Attr_EnvlopeArgs.AddRange(["0", "0"]); return; }//Rest is Concat Directly,No ENV
            Tuple<double, double>[] EnvlopeItem = new Tuple<double, double>[5];
            EnvlopeItem[0] = new Tuple<double, double>(0, 0);
            EnvlopeItem[1] = new Tuple<double, double>(Math.Max(5, pNote.Attributes.EnvFadeIn), pNote.AttrackVolume);
            EnvlopeItem[2] = new Tuple<double, double>(Math.Max(35, pNote.Attributes.EnvFadeOut), pNote.ReleaseVolume);
            EnvlopeItem[3] = new Tuple<double, double>(pNote.Attributes.TailOffset, 0);

            Attr_EnvlopeArgs.Clear();
            //0     5       55.1   0  100  100  0 17.415
            //p1    p2      p3     v1  v2  v3  v4   ovr
            //0   prevOvl  nextOvl 0  100  100 0  thisOvl
            Attr_EnvlopeArgs.Add(EnvlopeItem[0].Item1.ToString("F3"));//p1
            Attr_EnvlopeArgs.Add(EnvlopeItem[1].Item1.ToString("F3"));//p2
            Attr_EnvlopeArgs.Add(EnvlopeItem[2].Item1.ToString("F3"));//p3
            Attr_EnvlopeArgs.Add(EnvlopeItem[0].Item2.ToString("F3"));//v1
            Attr_EnvlopeArgs.Add(EnvlopeItem[1].Item2.ToString("F3"));//v2
            Attr_EnvlopeArgs.Add(EnvlopeItem[2].Item2.ToString("F3"));//v3
            Attr_EnvlopeArgs.Add(EnvlopeItem[3].Item2.ToString("F3"));//v4
            Attr_EnvlopeArgs.Add(pNote.Attributes.EnvFadeIn.ToString("F3"));//ovl
            Attr_EnvlopeArgs.Add(EnvlopeItem[3].Item1.ToString("F3"));//p4
        }
        private void UpdateFixedDuration()
        {
            double duration = pNote.DurationMSec;
            Attr_FixedDuration = string.Format("{0:0.000}@125{1}{2}", duration, pNote.Attributes.DurCorrection < 0 ? "" : "+", pNote.Attributes.DurCorrection.ToString("F3"));
        }
        public List<string> GetWavtoolArgs(string outputFile="temp.wav",bool isWindowsTool=true)
        {
            List<string> ret = new List<string>();
            ret.Add(CPath(outputFile, isWindowsTool));
            ret.Add(CPath(TempFilePath, isWindowsTool));
            ret.Add(pNote.Attributes.SkipOver.ToString("F3"));
            ret.Add(Attr_FixedDuration);
            ret.AddRange(Attr_EnvlopeArgs);
            return ret;
        }
        public List<string> GetResamplerArgs(bool isWindowsTool=true)
        {
            if (pNote.Attributes.IsRest) { return new List<string>(); }//Rest is Concat Directly,No ENV
            List<string> ResamplerArgs = new List<string>();
            ResamplerArgs.Add(CPath(pNote.Attributes.InputWavFile, isWindowsTool));
            ResamplerArgs.Add(CPath(TempFilePath, isWindowsTool));
            ResamplerArgs.AddRange(Attr_SynthesisArgs);
            return ResamplerArgs;
        }
        public string GetBatchBatItem(int index=1)
        {
            if (pNote.Attributes.IsRest) {
                List<string> args = new List<string>();
                args.Add(string.Format("\"{0}\"", CPath(TempFilePath, true)));
                args.Add("0");
                args.Add(Attr_FixedDuration);
                args.Add("0");
                args.Add("0");
                return "@\"%tool%\" \"%output%\" " + string.Join(" ", args);
            }

            StringBuilder Lines = new StringBuilder();
            
            Lines.AppendLine("@set params=" + string.Format("100 {0} !{1} {2}", pNote.Attributes.Modulation.ToString("F3"),pNote.Attributes.UltraPitchLine ? "625" : "125" ,pNote.Attributes.PitchLineString));
            Lines.AppendLine("@set env=" + string.Join(" ", Attr_EnvlopeArgs));
            Lines.AppendLine("@set vel=" + pNote.Attributes.Velocity.ToString("F3"));
            Lines.AppendLine("@set temp=" + string.Format("\"{0}\"", CPath(TempFilePath, true)));
            Lines.AppendLine("@set flag=\"" + pNote.Attributes.Flags + "\"");
            Lines.AppendLine("@set stp=" + pNote.Attributes.SkipOver.ToString("F3"));
            Lines.AppendLine("@call %helper% " + string.Join(" ", (new Func<List<string>>(() => {
                List<string> prms = new List<string>();
                prms.Add(string.Format("\"{0}\"",CPath(pNote.Attributes.InputWavFile)));
                prms.Add(pNote.Attributes.Tone);
                prms.Add(Attr_FixedDuration);
                prms.Add(pNote.Attributes.Preutter.ToString("F3"));//%4
                prms.Add(pNote.Attributes.Offset.ToString("F3"));//%5
                prms.Add(pNote.Attributes.DurRequired.ToString("F3"));//%6
                prms.Add(pNote.Attributes.Consonant.ToString("F3"));//%7
                prms.Add(pNote.Attributes.Cutoff.ToString("F3"));//%8
                prms.Add(index.ToString());//%9也没用
                return prms;
            }))()));
            return Lines.ToString();
        }

        public void UpdateExecutor()
        {
            UpdateSynthesisArgs();
            UpdateTempFileName();

            UpdateEnvlope();
            UpdateFixedDuration();
        }
    }
    public class URenderNote
    {
        public URenderNote(UPhonemeNote? parent=null)
        {
            Attributes = new URenderNote_Attributes(this);
            Executors = new URenderNote_RenderExecutor(this);
            this.Parent = parent;
        }
        public URenderNote? PrevNote { get; set; } = null;
        public URenderNote? NextNote { get; set; } = null;
        public Oto? RenderOto { get; set; } = null;
        public URenderNote_Attributes Attributes { get; set; }
        public URenderNote_RenderExecutor Executors { get; set; }
        public string VoiceBankPath { get; set; } = "";
        public double StartMSec { get; set; } = 0;
        public double DurationMSec { get; set; } = 0;
        public double EndMSec { get => StartMSec + DurationMSec; }
        public int NoteNumber { get; set; } = 60;
        public string Flags { get; set; } = "";
        public string EngineSalt { get; set; } = "";
        public int Velocity { get; set; } = 100;
        public double AttrackVolume { get; set; } = 100;
        public double ReleaseVolume { get; set; } = 100;
        public double OtoPreutterCorrected { get; set; } = 0;
        public double OtoOverlapCorrected { get; set; } = 0;
        public double OtoOffsetCorrected { get; set; } = 0;
        public double OtoCutoffCorrected { get; set; } = 0;
        public double OtoConsonantCorrected { get; set; } = 0;

        public UPhonemeNote? Parent { get; set; } = null;
    }
}
