using AudioLibrary.NAudio.Wave.SampleProviders;
using AudioLibrary.NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Science;
using TuneLab.Extensions.Voices;
using UtauSharpApi.UNote;
using UtauSharpApi.UPhonemizer;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;
using TuneLab.Base.Structures;
using ProtoBuf.WellKnownTypes;
using NWaves.FeatureExtractors.Multi;
using NWaves.Features;
using TuneLab.Base.Utils;
using NWaves.FeatureExtractors;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Collections.Concurrent;
using ProtoBuf.Meta;
using UtaubaseForTuneLab.AudioEffect;
using UtaubaseForTuneLab.Utils;
using static UtaubaseForTuneLab.UtauSynthesisTask;
using NWaves.Filters.Base;

namespace UtaubaseForTuneLab.UProjectGenerator
{
    internal static class UtauProject
    {
        public static UTaskProject ProcessSynthesizedPhonemes(this UTaskProject uTask)
        {
            for (int i = 0; i < uTask.Part.Notes.Count; i++)
            {
                var obj = uTask.Part.Notes[i].ObjectTag;
                var sNoteList = obj is List<ISynthesisNote> ? (List<ISynthesisNote>)obj : null;
                if (sNoteList == null) continue;

                List<SynthesizedPhoneme> phonemeList = new List<SynthesizedPhoneme>();
                foreach (var note in sNoteList)
                {
                    phonemeList.AddRange(note.Phonemes);
                }
                phonemeList = phonemeList.OrderBy(p => p.EndTime).ToList();
                var phonemes = phonemeList.OrderBy(p => p.StartTime);
                uTask.Part.Notes[i].PhonemeNotes = new List<UPhonemeNote>();
                for(int pi=0; pi < phonemeList.Count; pi++)
                {
                    var phoneme = phonemeList[pi];
                    var phonemeEnd = pi < phonemeList.Count - 1 ? phonemeList[pi + 1].StartTime : phonemeList.Last().EndTime;
                    uTask.Part.Notes[i].PhonemeNotes.Add(new UPhonemeNote(uTask.Part.Notes[i], phoneme.Symbol, (phonemeEnd - phoneme.StartTime) * 1000.0));
                }
            }
            return uTask;
        }
        public static UTaskProject ProcessPhonemizer(this UTaskProject uTask,IPhonemizer? PerferPhonemizer=null)
        {
            double minR = 120;//Hardcode As same as GenerateRenderPart
            IPhonemizer? phonemizer = PerferPhonemizer;
            if (phonemizer == null) phonemizer = uTask.Part.Phonemizer;
            if (phonemizer == null) phonemizer = new DefaultPhonemizer();
            for (int i = 0; i < uTask.Part.Notes.Count; i++)
            {
                List<UPhonemeNote> Processed;
                if(uTask.Part.Notes[i].Lyric.StartsWith(".") || uTask.Part.Notes[i].Lyric.StartsWith(".."))
                {
                    var newLyric = uTask.Part.Notes[i].Lyric.Substring(1);
                    newLyric=newLyric.StartsWith(".")? newLyric.Substring(1):newLyric;
                    Processed = new List<UPhonemeNote>() {
                        new UPhonemeNote(uTask.Part.Notes[i],newLyric,-1)
                    };
                }else
                {
                    Processed = phonemizer.Process(uTask.Part, i);
                }
                if(Processed.Count== uTask.Part.Notes[i].PhonemeNotes.Count)
                {
                    var TailFix = (
                        uTask.Part.Notes[i].PhonemeNotes.Count==1 //只有一个音素，无论如何都不管尾息（因为自己就是）
                        || (i< uTask.Part.Notes.Count-1 //非最后一个音符
                            && uTask.Part.Notes[i+1].StartMSec - uTask.Part.Notes[i].StartMSec <= minR //与后一个音符之间的间隙补偿（大于补偿会产生尾息）
                           )
                        )? 0: Processed.Last().SymbolMSec;//尾息修正

                    for(int pi=0;pi<Processed.Count;pi++)
                    {
                        if (Processed[pi].Equals(uTask.Part.Notes[i].PhonemeNotes[pi])) continue;
                        var fixedTailDiff = ((pi != Processed.Count - 2) ? 0 : TailFix);//为尾息前一个音符增加尾息长度
                        Processed[pi] = 
                            new UPhonemeNote(uTask.Part.Notes[i],
                            Processed[pi].Symbol,
                            uTask.Part.Notes[i].PhonemeNotes[pi].SymbolMSec + fixedTailDiff);
                    }
                }
                uTask.Part.Notes[i].PhonemeNotes = Processed;
            }
            return uTask;
        }

        public static double HeadPreSequenceMillsectionTime { get; private set; } =500;

        public static UTaskProject GenerateFrom(ISynthesisData data,VoiceBank vbanks,IRenderEngine renderEngine, RenderPart loop = RenderPart.FirstTrack)
        {
            UTaskProject ret = new UTaskProject();
            var mPart = ret.Part;
            mPart.SetVoiceBank(vbanks);
            mPart.ObjectTag = data;

            var baseStart = data.StartTime();
            var baseEnd = data.EndTime();

            var emptyTime = HeadPreSequenceMillsectionTime;//2s Empty;

            var pNote = data.Notes.GetEnumerator();
            int count = data.Notes.Count();
            while (pNote.MoveNext())
            {
                var note = pNote.Current;
                if (note.Lyric == "-")
                {
                    if (mPart.Notes.Count <= 0) continue;
                    double newEnd = (note.EndTime - baseStart) * 1000.0 + emptyTime;
                    double newLen = newEnd - mPart.Notes[mPart.Notes.Count - 1].StartMSec;//延长;
                    if (newLen > 0) mPart.Notes[mPart.Notes.Count - 1].DurationMSec = newLen;
                    var synthList=mPart.Notes[mPart.Notes.Count - 1].ObjectTag is List<ISynthesisNote> ? (List<ISynthesisNote>) mPart.Notes[mPart.Notes.Count - 1].ObjectTag : null;
                    if(synthList!=null)synthList.Add(note);
                }
                else
                {
                    UMidiNote uNote = mPart.createNote();
                    uNote.Lyric = note.Lyric;
                    string PrefixKey=note.Properties.GetString(UtauEngine.PrefixPairID,"AutoSelect");
                    if(loop==RenderPart.SecondTrack)
                    {
                        //第二轮次渲染设置
                        string XPrefixKey = note.Properties.GetString(UtauEngine.XTrack_PrefixPairID, "AutoSelect");
                        if (XPrefixKey != "AutoSelect") PrefixKey = XPrefixKey;
                    }
                    int PrefixOverlayNumber = vbanks.GetPrefixPairNoteNumber(PrefixKey);
                    uNote.NoteNumber = note.Pitch;
                    uNote.PrefixKey= PrefixOverlayNumber == -1 ? note.Pitch : PrefixOverlayNumber;
                    uNote.Phonemes = new List<string>();
                    uNote.Flags = renderEngine.GetNoteFlags(data, note,"",loop==RenderPart.SecondTrack);
                    uNote.Velocity=MathUtils.RoundLimit(note.Properties.GetDouble(UtauEngine.VelocityID,1)*100.0,0,200);
                    uNote.StartMSec = (note.StartTime - baseStart) * 1000.0 + emptyTime;
                    uNote.DurationMSec = note.Duration() * 1000.0;
                    uNote.ObjectTag = new List<ISynthesisNote>() { note };
                    if (uNote.DurationMSec > 0) mPart.Notes.Add(uNote);
                }
            }
            return ret;
        }
        public static SortedDictionary<double, double> PitchPrerender(ISynthesisData mData)
        {
            double audioStartTime = mData.StartTime()*1000.0 - UtauProject.HeadPreSequenceMillsectionTime;
            double startTime = Math.Max(0, audioStartTime);//实际获取Pit的开始点
            int emptyTime = (int)(startTime - audioStartTime);//头部填充的空白Pitch
            double endTime = mData.EndTime() * 1000.0;
            int duration = (int)(endTime - startTime)+1;//Pit总时长

            double[] times = new double[duration];
            for (int i = 0; i < duration; i++) { times[i] = Math.Round(startTime + i) / 1000.0; }//获取秒单位的Times组
            var absPitchs = mData.Pitch.GetValue(times);//获取绘制的绝对音高

            var values = new double[duration];//设置空白

            double transitionTime = 1000 * mData.PartProperties.GetDouble(UtauEngine.PitchTransitionTimeID, UtauEngine.PitchTransitionTimeConfig.DefaultValue);//设置过度长
            List<ISynthesisNote> mNotes=mData.Notes.ToList();
            for (ulong i = 0; i < (ulong)mNotes.Count() - 1; i++)
            {
                var note = mNotes[(int)i];
                var nextNote = mNotes[(int)(i + 1)];
                double midTime = Math.Min(note.EndTime * 1000.0, nextNote.StartTime * 1000.0) - startTime;
                int midTick = (int)Math.Round(midTime);//125BPM时1ms=1Tick，因此直接转换
                var pitchGap = nextNote.Pitch - note.Pitch;
                double transitionStartTime = midTime - transitionTime / 2;
                double transitionEndTime = midTime + transitionTime / 2;
                int transitionStartTick = ((int)Math.Round(transitionStartTime)).Limit(0, duration);
                int transitionEndTick = ((int)Math.Round(transitionEndTime)).Limit(0, duration);
                double transitionTicks = transitionEndTick - transitionStartTick;
                for (int t = transitionStartTick; t < transitionEndTick; t++)
                {
                    if (t < midTick)
                    {
                        values[t] = note.Pitch + MathUtility.CubicInterpolation((t - transitionStartTick) / transitionTicks) * pitchGap;
                    }
                    else
                    {
                        values[t] = nextNote.Pitch - (1 - MathUtility.CubicInterpolation((t - transitionStartTick) / transitionTicks)) * pitchGap;
                    }
                }
            }
            if (mNotes.Count > 0)
            {
                int noteIndex = 0;
                for (int i = 0; i < duration; i++)
                {
                    while(i > (int)(mNotes[noteIndex].EndTime*1000 - startTime))
                    {
                        noteIndex++;
                    }
                    double absPitch = absPitchs[i];
                    if (double.IsNaN(absPitch))
                    {
                        if (values[i]==0)
                        {
                            values[i] = mNotes[noteIndex].Pitch;
                        }
                    }else
                    {
                        values[i] = absPitch;
                    }

                }
            }
            SortedDictionary<double, double> ret = new SortedDictionary<double, double>();
            for (int i = 0; i < emptyTime; i++) ret.Add(i, values[0]);
            for(int i = 0; i < duration; i++)
            {
                ret.Add(i + emptyTime,values[i]);
            }
            return ret;
        }

        public static Dictionary<ISynthesisNote, SynthesizedPhoneme[]>? GetPhonemeInfoList(List<URenderNote> rPart)
        {
            try
            {
                var emptyTime = HeadPreSequenceMillsectionTime;//2s Empty;
                ISynthesisData? mData = null;
                mData = ((ISynthesisData)rPart.Where(rP => rP.Parent != null).First().Parent.Parent.Parent.ObjectTag);
                var baseStart = mData.StartTime() * 1000.0;

                Dictionary<ISynthesisNote, SynthesizedPhoneme[]> ret = new Dictionary<ISynthesisNote, SynthesizedPhoneme[]>();
                Dictionary<ISynthesisNote, int> retCount = new Dictionary<ISynthesisNote, int>();
                foreach (var rNote in rPart)
                {
                    if (rNote.Attributes.IsRest) continue;
                    double startMs = baseStart + rNote.StartMSec - emptyTime;
                    double endMs = rNote.DurationMSec + startMs;
                    var kNoteList = rNote.Parent.Parent.ObjectTag is List<ISynthesisNote> ? (List<ISynthesisNote>)rNote.Parent.Parent.ObjectTag : null;

                    double startTime = startMs / 1000.0;
                    double endTime = endMs / 1000.0;
                    foreach (var kNote in kNoteList)
                    {
                        if (kNote.StartTime > startTime) break;
                        if (kNote.EndTime < startTime) continue;

                        //ISynthesisNote? kNote = (ISynthesisNote?)rNote.Parent.Parent.ObjectTag;
                        var kCount = rNote.Parent.Parent.PhonemeNotes.Count;

                        if (!ret.ContainsKey(kNote))
                        {
                            ret.Add(kNote, new SynthesizedPhoneme[kCount]);
                            retCount.Add(kNote, 0);
                        }

                        ret[kNote][retCount[kNote]] = new SynthesizedPhoneme()
                        {
                            StartTime = startMs / 1000.0,
                            EndTime = endMs / 1000.0,
                            Symbol = rNote.Parent.Symbol
                        };

                        retCount[kNote]++;
                    }
                }
                foreach(var kv in ret)
                {
                    var l = (kv.Value.Where(p => p.StartTime > 0 && p.EndTime > 0)).ToArray();
                    if (kv.Value.Length != l.Length) ret[kv.Key] = l;
                }
                return ret;
            }
            catch { return null; }
        }

    }
    internal static class TaskHelper
    {
        public static void UpdateExecutors(List<URenderNote> rPart)
        {
            foreach (var rNote in rPart) { rNote.Executors.UpdateExecutor(); }
        }

        public static string GetPartRenderedFilePath(List<URenderNote> rPart, IRenderEngine renderEngine)
        {
            string GetMixedHash(List<string> hashes, string appendSalt = "")
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(appendSalt + string.Join("\r\n", hashes));
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
            List<string> infos = new List<string>();
            infos.Add(renderEngine.ResamplerPath);
            infos.Add(renderEngine.WavtoolPath);
            foreach (URenderNote rNote in rPart)
            {
                List<string> resamplerArgs = rNote.Executors.GetResamplerArgs();
                if (resamplerArgs.Count > 0) infos.Add(File.Exists(resamplerArgs[0]).ToString());
                infos.AddRange(resamplerArgs);
                infos.AddRange(rNote.Executors.GetWavtoolArgs("temp.wav"));
            }
            string hash = GetMixedHash(infos);
            string tmpPath = Path.Combine(Path.GetTempPath(), "UtauSharp", "PartRendered");
            if (!Directory.Exists(tmpPath)) { Directory.CreateDirectory(tmpPath); }
            return Path.Combine(tmpPath, string.Format("{0}.wav", hash));
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
        public static string CreateWorkdirWithBatchBat(string RenderWavPath, UTaskProject uTask, List<URenderNote> rPart)
        {
            string WorkDir = "";
            string ParentDir = Path.GetDirectoryName(RenderWavPath);
            string HelpName = Path.GetFileNameWithoutExtension(RenderWavPath);
            WorkDir = Path.Combine(ParentDir, string.Format("{0}_{1}", HelpName, GenerateRandomString(8)));
            if (!Directory.Exists(WorkDir)) { Directory.CreateDirectory(WorkDir); }
            string TempFile = Path.Combine(WorkDir, "temp.bat");
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.ReadWrite))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.WriteLine(uTask.Part.GetBatchBat());
                    int index = 0;
                    foreach (URenderNote rNote in rPart)
                    {
                        index++;
                        tw.WriteLine(rNote.Executors.GetBatchBatItem(index));
                    }
                }
            }
            return WorkDir;
        }

        public static string CreateCommandLine(string ExePath,List<string> Args,string WorkDir="",string UniqueFile="")
        {
            bool ContainsSpecialChars(string arg) { return arg.IndexOfAny(new char[] { '&', '|', '<', '>', ';', '^' }) != -1; }
            string EscapeSpecialChars(string arg)
            {
                StringBuilder sb = new StringBuilder();

                foreach (char c in arg)
                {
                    if (c == '&' || c == '|' || c == '<' || c == '>' || c == ';' || c == '^')
                    {
                        sb.Append('^');
                    }
                    sb.Append(c);
                }

                return sb.ToString();
            }
            string fmtArg(string p)
            {
                if (p.Length == 0) return "\"\"";
                if (p.Length > 1 && p.StartsWith("\"") && p.EndsWith("\"")) { return p; }
                if (p.Contains(' ') || p.Contains('\t') || ContainsSpecialChars(p)) return string.Format("\"{0}\"", p);
                if (p.Length>3 && p.ToUpper()[0]>='A' && p.ToUpper()[0]<='Z' && p[1]==':' && p[2]=='\\') return string.Format("\"{0}\"", EscapeSpecialChars(p));
                return p;
            }
            Args.Insert(0, ExePath);
            List<string> newArgs=Args.Select(p=>fmtArg(p.Trim())).ToList();
            string CommandLine = string.Join(" ", newArgs);
            if (WorkDir.Trim().Length <4) return CommandLine;
            StringBuilder sb = new StringBuilder();
            if (UniqueFile != "")
            {
                sb.AppendLine(string.Format("if not exist \"{0}\" (",UniqueFile));
            }
            sb.AppendLine(WorkDir.Substring(0, 2));
            sb.AppendLine(string.Format("cd \"{0}\"", WorkDir));
            sb.AppendLine(CommandLine);
            if (UniqueFile != "")
            {
                sb.AppendLine(")");
            }
            return sb.ToString();
        }

        public static void FinishWavTool(string OutputFile,string WorkDir="")
        {
            string ParentDir = Path.GetDirectoryName(OutputFile);
            string HelpName = Path.GetFileNameWithoutExtension(OutputFile);
            WorkDir = WorkDir=="" ? Path.Combine(ParentDir, string.Format("{0}_conf", HelpName)):WorkDir;

            //RemoveCacheFile
            if (Directory.Exists(WorkDir))
            {
                try
                {
                    Directory.Delete(WorkDir, true);
                }
                catch {; }
            }

            if (!File.Exists(OutputFile))
            {
                string whdFile = OutputFile+ ".whd";
                string datFile = OutputFile + ".dat";
                if (File.Exists(whdFile) && File.Exists(datFile))
                {
                    using (FileStream fs1 = new FileStream(whdFile, FileMode.Open, FileAccess.Read))
                    using (FileStream fs2 = new FileStream(datFile, FileMode.Open, FileAccess.Read))
                    using (FileStream outputFs = new FileStream(OutputFile, FileMode.Create, FileAccess.Write))
                    {
                        fs1.CopyTo(outputFs);
                        fs2.CopyTo(outputFs);
                    }
                    File.Delete(whdFile);
                    File.Delete(datFile);
                }
            }
        }

        public static TaskAudioData ReadWaveAudioData(string WavFile, double partStartTime)
        {

            double startTime = partStartTime * 1000.0 - UtauProject.HeadPreSequenceMillsectionTime;

            var reader = new WaveFileReader(WavFile);
            var srcProvider = reader.ToSampleProvider().ToMono();
            var sampleProvider = srcProvider;
            if (srcProvider.WaveFormat.SampleRate != 44100)
            {
                sampleProvider = new WdlResamplingSampleProvider(srcProvider, 44100);
            }

            int removeCount = 0;
            float[] tmp = new float[1] { 0 };
            while (tmp[0] == 0)
            {
                sampleProvider.Read(tmp, 0, 1);
                removeCount++;
            }

            double removedTime = (removeCount / 44100.0) * 1000.0;
            double absStartTime = startTime + removedTime;
            if (absStartTime < 0)
            {
                int jumpCount = (int)((-absStartTime / 1000.0) * 44100.0);
                float[] tmp2 = new float[jumpCount];
                sampleProvider.Read(tmp2, 0, jumpCount);
                sampleProvider.Read(tmp, 0, 1);
                removeCount = removeCount + jumpCount;
                absStartTime = 0;
            }

            int count = (int)reader.SampleCount - removeCount + 1;
            float[] audioData = new float[count];
            audioData[0] = tmp[0];
            sampleProvider.Read(audioData, 1, count);
            reader.Close();

            TaskAudioData ret = new TaskAudioData()
            {
                audio_Data = audioData,
                audio_StartMillsec = absStartTime,
                audio_SampleRate = 44100.0
            };
            return ret;
        }
        public static int FindAudioSampleIndex(TaskAudioData audioData,double time_second,double partStartTime_second)
        {
            var emptyMs = audioData.audio_StartMillsec - partStartTime_second * 1000.0 + UtauProject.HeadPreSequenceMillsectionTime;
            return (int)Math.Round((time_second - emptyMs / 1000.0) * audioData.audio_SampleRate);
        }

        public static List<List<Point>> WaveAudioDataPitchDetect(TaskAudioData audioInfo)
        {
            List<List<Point>> ret = new List<List<Point>>();

            //NWaves.FeatureExtractors.PitchExtractor
            var pExtractor = new TimeDomainFeaturesExtractor(new NWaves.FeatureExtractors.Options.MultiFeatureOptions() { 
                SamplingRate = (int)audioInfo.audio_SampleRate
               // FrameSize=1024
            });
            pExtractor.AddFeature("pitch", (s, start, end) => {
                //return Pitch.FromAutoCorrelation(
                return Pitch.FromYin(
                    s, start, end, 80, 8000);
            });
            var freqPitches = pExtractor.ComputeFrom(audioInfo.audio_Data);
            var pitCtlTimes = pExtractor.TimeMarkers(freqPitches.Count, audioInfo.audio_StartMillsec / 1000.0);

            double getNoteNumber(double frequency){

                double ratio = frequency / 440.0;
                double log2Ratio = Math.Log(ratio, 2);
                return 69 + 12 * log2Ratio;
            }


            List<List<Point>> wavPitLines = new List<List<Point>>();
            List<Point> wavPitLine = new List<Point>();
            for(int pi=0; pi<freqPitches.Count;pi++)
            {
                double tSec = pitCtlTimes[pi];
                double pn = getNoteNumber(freqPitches[pi][4]);

                if(double.IsNaN(pn) || double.IsInfinity(pn))
                {
                    if(wavPitLine.Count>0)
                    {
                        wavPitLines.Add(wavPitLine);
                        wavPitLine = new List<Point>();
                    }
                }else wavPitLine.Add(new Point() { X = tSec, Y = pn });
            }
            if (wavPitLine.Count > 0)
            {
                wavPitLines.Add(wavPitLine);
                wavPitLine = new List<Point>();
            }
            return wavPitLines;
        }
        public static void WaitForFileRelease(string filePath)
        {
            bool IsFileLocked(string filePath)
            {
                try
                {
                    using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        stream.Close();
                    }
                }
                catch (IOException)
                {
                    return true;
                }
                return false;
            }
            while (IsFileLocked(filePath))
            {
                Task.Delay(500).Wait();
            }
        }
    }

    internal class TaskAudioData
    {
        public double audio_SampleRate { get; set; } = 44100.0;
        public double audio_StartMillsec { get; set; } = 0;
        public double audio_DurationMillsec => ((audio_Data.Length / audio_SampleRate) * 1000.0);
        public float[] audio_Data { get; set; } = new float[0];
    }
}
