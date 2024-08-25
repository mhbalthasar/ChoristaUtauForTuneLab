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

namespace UtaubaseForTuneLab.UProjectGenerator
{
    internal static class UtauProject
    {
        public static UTaskProject ProcessPhonemizer(this UTaskProject uTask,IPhonemizer PerferPhonemizer=null)
        {
            IPhonemizer phonemizer = PerferPhonemizer;
            if (phonemizer == null) phonemizer = uTask.Part.Phonemizer;
            if (phonemizer == null) phonemizer = new DefaultPhonemizer();
            for (int i = 0; i < uTask.Part.Notes.Count; i++)
            {
                uTask.Part.Notes[i].PhonemeNotes = phonemizer.ProcessEx(uTask.Part.VoiceBank, uTask.Part, i);
            }
            return uTask;
        }

        public static double HeadPreSequenceMillsectionTime { get; private set; } =500;

        public static UTaskProject GenerateFrom(ISynthesisData data,VoiceBank vbanks)
        {
            UTaskProject ret = new UTaskProject();
            var mPart = ret.Part;
            mPart.SetVoiceBank(vbanks);
            mPart.Tempo = 125.0;//125bpm alias 1tick=1ms

            var baseStart = data.StartTime();
            var baseEnd = data.EndTime();

            var emptyTime = HeadPreSequenceMillsectionTime;//2s Empty;

            var pNote = data.Notes.GetEnumerator();
            int count = data.Notes.Count();
            while(pNote.MoveNext()){
                var note = pNote.Current;
                if(note.Lyric =="-")
                {
                    double newEnd = (note.EndTime - baseStart) * 1000.0 + emptyTime;
                    mPart.Notes[mPart.Notes.Count - 1].DurationMSec = newEnd - mPart.Notes[mPart.Notes.Count - 1].StartMSec;//延长
                }else
                {
                    UMidiNote uNote = mPart.createNote();
                    uNote.Lyric = note.Lyric;
                    uNote.NoteNumber = note.Pitch;
                    uNote.Phonemes = new List<string>();
                    uNote.StartMSec = (note.StartTime - baseStart) * 1000.0 + emptyTime;
                    uNote.DurationMSec = note.Duration() * 1000.0;
                    mPart.Notes.Add(uNote);
                }
            }
            return ret;
        }
        public static SortedDictionary<int, double> PitchPrerender(ISynthesisData data)
        {
            SortedDictionary<int, double> ret = new SortedDictionary<int, double>();

            double spt = 0;

            double preseq = HeadPreSequenceMillsectionTime;
            double startTime = data.StartTime() * 1000.0;
            double endTime = data.EndTime() * 1000.0;
            if (preseq > startTime)
            {
                if (startTime < preseq)
                {
                    spt = preseq - startTime;
                    preseq -= spt;
                }
                double pt = data.Pitch.GetValue(new List<double> { 0 })[0];
                double pn = data.Notes.First().Pitch;
                if (!double.IsNaN(pt)) pn = pt;
                for (int i = 0; i < spt; i++) ret.Add(i, pn);
            }

            double startAbsTime = startTime - preseq;
            double durationAbs = endTime - startAbsTime + 1;
            double[] times = new double[(int)durationAbs];
            for (int i = 0; i < (int)durationAbs; i++) { times[i] = (startAbsTime + i)/1000.0; }

            double[] pitchs = new double[(int)durationAbs];

            ISynthesisNote[] sNotes = data.Notes.Where(p => (
              p.StartTime < endTime && p.EndTime >= startAbsTime
            )).ToArray();

            foreach (var note in sNotes)
            {
                for (int i = 0; i < note.Duration() * 1000; i++)
                {
                    double pi = note.StartTime * 1000.0 + i;
                    if (pi >= startAbsTime && pi <= (int)endTime)
                    {
                        int idx = (int)(pi - startAbsTime);
                        pitchs[idx] = note.Pitch;
                    }
                }
            }


            double smoothTime = 120;//ms
            for (int i = 1; i < sNotes.Length; i++)
            {
                double harfSt = smoothTime / 2;
                var prevNote = sNotes[i - 1];
                var curNote = sNotes[i];
                double pst = Math.Min(prevNote.Duration() * 1000.0 / 2, harfSt);
                double cst = Math.Min(curNote.Duration() * 1000.0 / 2, harfSt);
                double p1x = prevNote.EndTime * 1000.0 - pst - startAbsTime;
                double p2x = curNote.StartTime * 1000.0 + cst - startAbsTime;
                double p1y = prevNote.Pitch;
                double p2y = curNote.Pitch;
                double gap = p2y - p1y;
                int gax = (int)(p2x - p1x);
                int midx = (int)((p2x + p1x) / 2);
                for (int t = (int)p1x; t < (int)p2x; t++)
                {
                    if (t < midx)
                    {
                        pitchs[t] = p1y + MathUtility.CubicInterpolation((t - p1x) / gax) * gap;
                    }
                    else
                    {
                        pitchs[t] = p2y - (1 - MathUtility.CubicInterpolation((t - p1x) / gax)) * gap;
                    }
                }
            }

            double[] abspit = data.Pitch.GetValue(times);
            for (int i = 0; i < abspit.Length; i++)
            {
                if (!double.IsNaN(abspit[i])) pitchs[i] = abspit[i];
                else if (double.IsNaN(pitchs[i]) || pitchs[i] == 0)
                {
                    double nearp = double.NaN;
                    for (int j = i + 1; j < pitchs.Length; j++) { if (!double.IsNaN(pitchs[j]) && pitchs[j] > 0) { nearp = pitchs[j]; break; } }
                    if (!double.IsNaN(nearp)) for (int j = i; j < pitchs.Length; j++) { if (double.IsNaN(pitchs[j]) || pitchs[j] == 0) pitchs[j] = nearp; else break; }
                }
            }

            for (int t = 0; t < pitchs.Length; t++)
            {
                int Key = (int)(t + spt);
                if (ret.ContainsKey(Key)) ret[Key] = pitchs[t];
                else ret.Add(Key, pitchs[t]);
            }
            return ret;
        }


        public static SortedDictionary<int, double> PitchPrerender2(ISynthesisData mData)
        {
            double audioStartTime = mData.StartTime()*1000.0 - UtauProject.HeadPreSequenceMillsectionTime;
            double startTime = Math.Max(0, audioStartTime);//实际获取Pit的开始点
            int emptyTime = (int)(startTime - audioStartTime);//头部填充的空白Pitch
            double endTime = mData.EndTime() * 1000.0;
            int duration = (int)(endTime - startTime)+1;//Pit总时长

            double[] times = new double[duration];
            for (int i = 0; i < duration; i++) { times[i] = (startTime + i) / 1000.0; }//获取秒单位的Times组
            var absPitchs = mData.Pitch.GetValue(times);//获取绘制的绝对音高

            var values = new double[duration];//设置空白

            double transitionTime = 1000 * mData.PartProperties.GetDouble(UtauEngine.PitchTransitionTimeID, UtauEngine.PitchTransitionTimeConfig.DefaultValue);//设置过度长
            List<ISynthesisNote> mNotes=mData.Notes.ToList();
            for (ulong i = 0; i < (ulong)mNotes.Count() - 1; i++)
            {
                var note = mNotes[(int)i];
                var nextNote = mNotes[(int)(i + 1)];
                double midTime = Math.Min(note.EndTime * 1000.0, nextNote.StartTime * 1000.0);
                int midTick = (int)(midTime);//125BPM时1ms=1Tick，因此直接转换
                var pitchGap = nextNote.Pitch - note.Pitch;
                double transitionStartTime = midTime - transitionTime / 2;
                double transitionEndTime = midTime + transitionTime / 2;
                int transitionStartTick = ((int)transitionStartTime).Limit(0, duration);
                int transitionEndTick = ((int)transitionEndTime).Limit(0, duration);
                double transitionTicks = transitionEndTick - transitionStartTick;
                for (int t = transitionStartTick; t < transitionEndTick; t++)
                {
                    if (t < midTick)
                    {
                        values[t] += MathUtility.CubicInterpolation((t - transitionStartTick) / transitionTicks) * pitchGap;
                    }
                    else
                    {
                        values[t] -= (1 - MathUtility.CubicInterpolation((t - transitionStartTick) / transitionTicks)) * pitchGap;
                    }
                }
            }
            float[] basePitch = new float[mNotes.Count+ 1];
            int[] basePitchIndex = new int[mNotes.Count + 1];
            for (ulong i = 0; i < (ulong)mNotes.Count; i++)
            {
                var note = mNotes[(int)i];
                basePitch[i] = note.Pitch;
                basePitchIndex[i] = (int)(note.EndTime*1000.0);
                if (i + 1 != (ulong)mNotes.Count()) 
                    basePitchIndex[i] = Math.Min(basePitchIndex[i], (int)(mNotes[(int)i + 1].StartTime*1000.0));
            }
            if (mNotes.Count > 0)
            {
                basePitch[mNotes.Count] = basePitch[mNotes.Count - 1];
                basePitchIndex[mNotes.Count] = duration;
                int currentIndex = 0;
                for (int i = 0; i < duration; i++)
                {
                    while (i >= basePitchIndex[currentIndex])
                    {
                        currentIndex++;
                    }
                    double absPitch = absPitchs[i];
                    if (double.IsNaN(absPitch))
                    {
                        values[i] += basePitch[currentIndex];
                    }else
                    {
                        values[i] = absPitch;
                    }

                }
            }
            SortedDictionary<int, double> ret = new SortedDictionary<int, double>();
            for (int i = 0; i < emptyTime; i++) ret.Add(i, values[0]);
            for(int i = 0; i < duration; i++)
            {
                ret.Add(i + emptyTime,values[i]);
            }
            return ret;
        }


    }   
    internal static class TaskHelper {

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
                List<string> resamplerArgs = rNote.GetResamplerArgs();
                if (resamplerArgs.Count > 0) infos.Add(File.Exists(resamplerArgs[0]).ToString());
                infos.AddRange(resamplerArgs);
                infos.AddRange(rNote.GetWavToolArgs("temp.wav"));
            }
            string hash = GetMixedHash(infos);
            string tmpPath = Path.Combine(Path.GetTempPath(), "UtauSharp", "PartRendered");
            if (!Directory.Exists(tmpPath)) { Directory.CreateDirectory(tmpPath); }
            return Path.Combine(tmpPath, string.Format("{0}.wav", hash));
        }
        public static string CreateWorkdirWithBatchBat(string RenderWavPath, UTaskProject uTask, List<URenderNote> rPart)
        {
            string WorkDir = "";
            string ParentDir = Path.GetDirectoryName(RenderWavPath);
            string HelpName = Path.GetFileNameWithoutExtension(RenderWavPath);
            WorkDir = Path.Combine(ParentDir, string.Format("{0}_conf", HelpName));
            if (!Directory.Exists(WorkDir)) { Directory.CreateDirectory(WorkDir); }
            string TempFile = Path.Combine(WorkDir, "temp.bat");
            using (FileStream fs = new FileStream(TempFile, FileMode.Create, FileAccess.ReadWrite))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.WriteLine(uTask.Part.GetBatchBat());
                    foreach (URenderNote rNote in rPart)
                    {
                        tw.WriteLine(rNote.GetBatchBat());
                    }
                }
            }
            return WorkDir;
        }
    
        public static void FinishWavTool(string OutputFile)
        {   
            string ParentDir = Path.GetDirectoryName(OutputFile);
            string HelpName = Path.GetFileNameWithoutExtension(OutputFile);
            string WorkDir = Path.Combine(ParentDir, string.Format("{0}_conf", HelpName));

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
                string whdFile = Path.Combine(ParentDir, HelpName + ".whd");
                string datFile = Path.Combine(ParentDir, HelpName + ".dat");
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

        public static TaskAudioData ReadWaveAudioData(string WavFile,double partStartTime)
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

        public static List<List<Point>> WaveAudioDataPitchDetect(TaskAudioData audioInfo)
        {
            double CtlSpacingMs = 25;// 50;
            int CtlSpacingSamples = (int)((CtlSpacingMs / 1000.0) * audioInfo.audio_SampleRate);
            int ptrSampleIndex = 0;
            double ptrCurrentTimeMs = 0;
            List<List<Point>> wavPitLines = new List<List<Point>>();
            List<Point> wavPitLine = new List<Point>();
            while (ptrSampleIndex + CtlSpacingSamples <= audioInfo.audio_Data.Length)
            {
                double pointPn = YINSharp.PnFromYin(audioInfo.audio_Data, (int)audioInfo.audio_SampleRate, ptrSampleIndex, ptrSampleIndex + CtlSpacingSamples);
                ptrSampleIndex += CtlSpacingSamples;
                ptrCurrentTimeMs += CtlSpacingMs;
                if (double.IsNaN(pointPn) || double.IsInfinity(pointPn))
                {
                    if (wavPitLine.Count > 0)
                    {
                        wavPitLines.Add(wavPitLine);
                        wavPitLine = new List<Point>();
                    }
                    continue;
                }
                wavPitLine.Add(new Point() { X = (audioInfo.audio_StartMillsec + ptrCurrentTimeMs - CtlSpacingMs) / 1000.0, Y = pointPn });
            }
            if (wavPitLine.Count > 0)
            {
                wavPitLines.Add(wavPitLine);
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
