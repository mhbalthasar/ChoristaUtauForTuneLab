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

                //set pitchs
                var pitchtable = UtauProject.PitchPrerender2(synthesisData);
                foreach (URenderNote rNote in rPart)
                {
                    rNote.SetPitchBends((ms_times) =>
                    {
                        double[] ret=new double[ms_times.Length];
                        for(int i=0;i< ms_times.Length;i++)
                        {
                            int timeKey = (int)ms_times[i];
                            if(pitchtable.ContainsKey(timeKey))ret[i] = (double)pitchtable[timeKey];
                            else  ret[i] = 0;//BUG 
                        }
                        return ret;
                    });
                }

                //PrepareHash
                string OutputFile = TaskHelper.GetPartRenderedFilePath(rPart, renderEngine);

                if (!File.Exists(OutputFile))
                {
                    //resampler
                    foreach (URenderNote rNote in rPart)
                    {
                        string resampler_exe = renderEngine.ResamplerPath;
                        List<string> args = rNote.GetResamplerArgs();
                        if (args.Count == 0) continue;
                        if (File.Exists(rNote.TempFilePath)) continue;
                        Process p = wine.CreateWineProcess(resampler_exe, args);
                        p.Start();
                        p.WaitForExit();
                    }

                    //wavtool
                    string WorkDir = TaskHelper.CreateWorkdirWithBatchBat(OutputFile,uTask,rPart);
                    foreach (URenderNote rNote in rPart)
                    {
                        string wavtool_exe = renderEngine.WavtoolPath;
                        List<string> args = rNote.GetWavToolArgs(OutputFile);
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
                    CompleteTask(OutputFile);
                }
            });
        }

        private void CompleteTask(string OutputWav)
        {
            var audioInfo=TaskHelper.ReadWaveAudioData(OutputWav,synthesisData.StartTime());

            var pitLines= TaskHelper.WaveAudioDataPitchDetect(audioInfo);
            var ret = new SynthesisResult(audioInfo.audio_StartMillsec/1000.0, 44100, audioInfo.audio_Data, pitLines);

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
