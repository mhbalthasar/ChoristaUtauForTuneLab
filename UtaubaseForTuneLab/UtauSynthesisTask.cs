using AudioLibrary.NAudio.Wave.SampleProviders;
using AudioLibrary.NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Science;
using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab.UProjectGenerator;
using UtauSharpApi.UNote;
using UtauSharpApi.UPhonemizer;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;
using TuneLab.Base.Structures;
using System.Formats.Tar;

namespace UtaubaseForTuneLab
{
    internal class UtauSynthesisTask(
            ISynthesisData synthesisData,
            IRenderEngine renderEngine,
            VoiceBank voiceBank,
            IPhonemizer? phonemizer
        ) : ISynthesisTask
    {
        public event Action<SynthesisResult>? Complete;
        public event Action<double>? Progress;
        public event Action<string>? Error;

        public void Resume()
        {
            return;
        }

        public void SetDirty(string dirtyType)
        {
            return;
        }

        public void Start()
        {
            WineHelper wine = new WineHelper();
            Task.Run(() =>
            {
                UTaskProject uTask = UtauProject.GenerateFrom(synthesisData,voiceBank).ProcessPhonemizer(phonemizer);
                List<URenderNote> rPart = uTask.Part.GenerateRendPart(renderEngine.EngineUniqueString);

                Dictionary<ISynthesisNote, SynthesizedPhoneme[]> phonemeInfoList = UtauProject.GetPhonemeInfoList(rPart);
                //set pitchs
                var pitchtable = UtauProject.PitchPrerender(synthesisData);
                foreach (URenderNote rNote in rPart)
                {
                    rNote.Attributes.SetPitchLine((ms_times) =>
                    {
                        double[] ret=new double[ms_times.Length];
                        for (int i = 0; i < ms_times.Length; i++)
                        {
                            double timeKey = ms_times[i];
                            var nextP = pitchtable.Where(x => x.Key >= timeKey).FirstOrDefault(new KeyValuePair<double, double>(-1, -1));
                            var prevP = pitchtable.Where(x => x.Key <= timeKey).LastOrDefault(new KeyValuePair<double, double>(-1, -1));
                            if (nextP.Key == -1 && prevP.Key == -1) ret[i] = 0;
                            else if (nextP.Key == -1) ret[i] = prevP.Value;
                            else if (prevP.Key == -1) ret[i] = nextP.Value;
                            else if (nextP.Key == prevP.Key) ret[i] = prevP.Value;
                            else ret[i] = MathUtility.LineValue(prevP.Key, prevP.Value, nextP.Key, nextP.Value, timeKey);
                        }
                        return ret;
                    });
                }

                //Generate All Args
                TaskHelper.UpdateExecutors(rPart);

                //PrepareHash
                string OutputFile = TaskHelper.GetPartRenderedFilePath(rPart, renderEngine);

                if (!File.Exists(OutputFile))
                {
                    //resampler
                    foreach (URenderNote rNote in rPart)
                    {
                        string resampler_exe = renderEngine.ResamplerPath;
                        List<string> args = rNote.Executors.GetResamplerArgs(true);
                        if (args.Count == 0) continue;
                        if (File.Exists(rNote.Executors.TempFilePath)) continue;
                        Process p = wine.CreateWineProcess(resampler_exe, args);
                        p.Start();
                        p.WaitForExit();
                    }

                    //wavtool
                    string WorkDir = TaskHelper.CreateWorkdirWithBatchBat(OutputFile,uTask,rPart);
                    foreach (URenderNote rNote in rPart)
                    {
                        string wavtool_exe = renderEngine.WavtoolPath;
                        List<string> args = rNote.Executors.GetWavtoolArgs(OutputFile,true);
                        Process p = wine.CreateWineProcess(wavtool_exe, args, WorkDir);
                        p.Start();
                        p.WaitForExit();
                    }

                }

                TaskHelper.FinishWavTool(OutputFile);

                if(File.Exists(OutputFile))
                {
                    TaskHelper.WaitForFileRelease(OutputFile);
                    try
                    {
                        var reader = new WaveFileReader(OutputFile);
                        reader.Close();
                    }
                    catch {File.Delete(OutputFile);Error?.Invoke("Rendered File Cannot Readable"); return; }
                    CompleteTask(OutputFile,pitchtable,phonemeInfoList);
                }
            });
        }

        private List<List<Point>> FormatPitchLines(SortedDictionary<double, double> pitchLines)
        {
            List<List<Point>> pitLines = new List<List<Point>>();
            List<Point> pPLine = new List<Point>();
            foreach (var kv in pitchLines)
            {
                pPLine.Add(new Point() { X = (synthesisData.StartTime() - UtauProject.HeadPreSequenceMillsectionTime / 1000.0) + (kv.Key / 1000.0), Y = kv.Value });
            }
            pitLines.Add(pPLine);
            return pitLines;
        }

        private void CompleteTask(string OutputWav, SortedDictionary<double, double> pitchLines, Dictionary<ISynthesisNote, SynthesizedPhoneme[]>? phonemeInfoList=null)
        {
            var audioInfo=TaskHelper.ReadWaveAudioData(OutputWav,synthesisData.StartTime());

            //var pitLines= TaskHelper.WaveAudioDataPitchDetect2(audioInfo);
            var pitLines = FormatPitchLines(pitchLines);

            var ret = new SynthesisResult(audioInfo.audio_StartMillsec/1000.0, 44100, audioInfo.audio_Data, pitLines,phonemeInfoList);

            Complete?.Invoke(ret);
        }

        public void Stop()
        {
            return;
        }

        public void Suspend()
        {
            return;
        }
    }
}
