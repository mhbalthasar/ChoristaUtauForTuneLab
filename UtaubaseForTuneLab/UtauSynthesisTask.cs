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
using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using TuneLab.Base.Structures;
using System.Formats.Tar;
using System.Xml.Linq;
using UtaubaseForTuneLab.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UtaubaseForTuneLab
{
    internal class UtauSynthesisTask(
            ISynthesisData synthesisData,
            IRenderEngine renderEngine,
            VoiceBank voiceBank
        ) : ISynthesisTask
    {
        public event Action<SynthesisResult>? Complete;
        public event Action<double>? Progress;
        public event Action<string>? Error;

        private bool mCacnel = false;

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
            Task.Run(() =>
            {
                StartRenderTask();
            });
        }

        public enum RenderPart
        {
            FirstTrack,
            SecondTrack
        }
        private bool bHaveXTrack { get
            {
                foreach(var note in synthesisData.Notes)
                {
                    if (note.Lyric == "-") continue;
                    string XPrefixKey = note.Properties.GetString(UtauEngine.PrefixPairID, "AutoSelect");
                    string PrefixKey = note.Properties.GetString(UtauEngine.XTrack_PrefixPairID, "AutoSelect");
                    if (XPrefixKey.ToLower() == "AutoSelect") continue;
                    if (XPrefixKey == PrefixKey) continue;
                    AutoPropertyGetter apg = new AutoPropertyGetter(synthesisData, UtauEngine.XTrack_XPrefixKeyID, 0,1,0);
                    double XPKV=apg.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(note), AutoPropertyGetter.PropertyType.FullBar, AutoPropertyGetter.ValueSelectType.FarZero);
                    if (XPKV > 0) 
                        return true;
                }
                return false;
            } 
        }
        private void StartRenderTask(RenderPart loop = RenderPart.FirstTrack, string? firstTrackAudioFile = null)
        {
            WineHelper wine = new WineHelper();

            IPhonemizer? phonemizer = null;
            string phonemizerKey=synthesisData.PartProperties.GetString(UtauEngine.PhonemizerSelectorID, "AutoSelect");
            if (phonemizerKey != "AutoSelect")
            {
                phonemizer = PhonemizerSelector.BuildPhonemizer(phonemizerKey, voiceBank);
                PhonemizerSelector.SaveLastPhonemizer(phonemizerKey, voiceBank);
            }

            UTaskProject uTask = UtauProject.GenerateFrom(synthesisData, voiceBank, renderEngine, loop)
                .ProcessSynthesizedPhonemes()
                .ProcessPhonemizer(phonemizer);
            List<URenderNote> rPart = uTask.Part.GenerateRendPart(
                renderEngine.EngineUniqueString,
                new Func<URenderNote, URenderNote>((iNote) =>
                {
                    if (iNote.Attributes.IsRest) return iNote;
                    if(loop==RenderPart.FirstTrack){//只在第一循环有用
                        //OtoFix
                        AutoPropertyGetter ooc = new AutoPropertyGetter(synthesisData, UtauEngine.OtoOverlapFixedID, -100,100, 0);
                        AutoPropertyGetter opc = new AutoPropertyGetter(synthesisData, UtauEngine.OtoPreutterFixedID, -100,100,0);
                        iNote.OtoOverlapCorrected = MathUtils.Limit(ooc.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackPoint, AutoPropertyGetter.ValueSelectType.NearZero), -100, 100);
                        iNote.OtoPreutterCorrected = MathUtils.Limit(opc.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackPoint, AutoPropertyGetter.ValueSelectType.NearZero), -100, 100);

                        AutoPropertyGetter olb = new AutoPropertyGetter(synthesisData, UtauEngine.OtoLeftBFixedID, -100, 100, 0);
                        AutoPropertyGetter orb = new AutoPropertyGetter(synthesisData, UtauEngine.OtoRightBFixedID, -100, 100, 0);
                        iNote.OtoOffsetCorrected = MathUtils.Limit(olb.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackPoint, AutoPropertyGetter.ValueSelectType.NearZero), -100, 100);
                        iNote.OtoCutoffCorrected = MathUtils.Limit(orb.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackPoint, AutoPropertyGetter.ValueSelectType.NearZero), -100, 100);

                        AutoPropertyGetter ofl = new AutoPropertyGetter(synthesisData, UtauEngine.OtoFixedLengthFixedID, -100, 100, 0);
                        iNote.OtoConsonantCorrected = MathUtils.Limit(ofl.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackPoint, AutoPropertyGetter.ValueSelectType.NearZero), -100, 100);
                    }
                    return iNote;
                }),
                new Func<URenderNote, URenderNote>((iNote) =>
                {
                    if (iNote.Attributes.IsRest) return iNote;
                    //Setup Attrack&Release
                    {
                        AutoPropertyGetter atk = new AutoPropertyGetter(synthesisData, UtauEngine.AttrackID, 0, 1, 1);
                        AutoPropertyGetter rle = new AutoPropertyGetter(synthesisData, UtauEngine.ReleaseID, 0, 1, 1);
                        iNote.AttrackVolume = MathUtils.Limit(atk.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.AttrackBar, AutoPropertyGetter.ValueSelectType.NearZero) * 100.0, 0, 100);
                        iNote.ReleaseVolume = MathUtils.Limit(rle.GetNoteBarValue(new AutoPropertyGetter.VirtualNote(synthesisData, iNote), AutoPropertyGetter.PropertyType.ReleaseBar, AutoPropertyGetter.ValueSelectType.NearZero) * 100.0, 0, 100);
                    }
                    //Setup Flags
                    {
                        var bar = new AutoPropertyGetter.VirtualNote(synthesisData, iNote);
                        var area = bar.GetTimeArea(AutoPropertyGetter.PropertyType.FullBar);
                        iNote.Flags = renderEngine.GetTimeFlags(synthesisData, area.Item1, iNote.Flags, area.Item2 - area.Item1,loop == RenderPart.SecondTrack);
                    }
                    return iNote;
                })
            );

            //set pitchs
            var pitchtable = UtauProject.PitchPrerender(synthesisData);
            var pitResolution = synthesisData.PartProperties.GetString(UtauEngine.PitchResolutionID, "Ultra");
            int pitResolKey;
            switch (pitResolution)
            {
                case "Normal":pitResolKey = 1; break;
                case "Electronic":pitResolKey = 2;break;
                default:pitResolKey = renderEngine.SupportUltraTempo?0:1;break;
            }
            foreach (URenderNote rNote in rPart)
            {
                rNote.Attributes.SetPitchLine((ms_times) =>
                {
                    double[] ret = new double[ms_times.Length];
                    for (int i = 0; i < ms_times.Length; i++)
                    {
                        double timeKey = ms_times[i];
                        var nextP = pitchtable.Where(x => x.Key >= timeKey).FirstOrDefault(new KeyValuePair<double, double>(-1, -1));
                        var prevP = pitchtable.Where(x => x.Key <= timeKey).LastOrDefault(new KeyValuePair<double, double>(-1, -1));
                        if (pitResolKey == 2)
                        { //ROBOT
                            prevP = new KeyValuePair<double, double>(prevP.Key, Math.Round(prevP.Value));
                            nextP = new KeyValuePair<double, double>(nextP.Key, Math.Round(nextP.Value));
                        }
                        if (nextP.Key == -1 && prevP.Key == -1) ret[i] = 0;
                        else if (nextP.Key == -1) ret[i] = pitchtable[prevP.Key];
                        else if (prevP.Key == -1) ret[i] = pitchtable[nextP.Key];
                        else if (nextP.Key == prevP.Key) ret[i] = prevP.Value;
                        else ret[i] = MathUtility.LineValue(prevP.Key, prevP.Value, nextP.Key, nextP.Value, timeKey);
                    }
                    return ret;
                },pitResolKey==0 );
            }

            //Generate All Args
            TaskHelper.UpdateExecutors(rPart);

            //PrepareHash
            string OutputFile = TaskHelper.GetPartRenderedFilePath(rPart, renderEngine);
            FileLocker.WaitforLock(OutputFile);
            //  lock (MutexObject)
            {
                if (!File.Exists(OutputFile))
                {
                    //PrepareWorkDir
                    string WorkDir = TaskHelper.CreateWorkdirWithBatchBat(OutputFile, uTask, rPart);

                    //resampler

                    if (WineHelper.UnderWine)
                    {
                        StringBuilder resampleBatContent = new StringBuilder();
                        string resampleBatFile = Path.Combine(WorkDir, "resampler.bat");
                        resampleBatContent.AppendLine("chcp 932");//CodePage:Shift-JIS
                        foreach (URenderNote rNote in rPart)
                        {
                            string resampler_exe = renderEngine.ResamplerPath;
                            List<string> args = rNote.Executors.GetResamplerArgs(true);
                            if (args.Count == 0) continue;
                            if (File.Exists(rNote.Executors.TempFilePath)) continue;
                            resampleBatContent.AppendLine(TaskHelper.CreateCommandLine(resampler_exe, args, WorkDir, args[1]));
                        }

                        File.WriteAllText(resampleBatFile, resampleBatContent.ToString(), CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS"));
                        if (File.Exists(resampleBatFile))
                        {
                            Process wavP = wine.CreateWineProcess("cmd.exe", new List<string>(["/c", resampleBatFile]));
                            wavP.Start();
                            wavP.WaitForExit();
                            File.Delete(resampleBatFile);
                        }
                    }
                    else
                    {
                        Dictionary<string, Process> resampleProcess = new Dictionary<string, Process>();
                        foreach (URenderNote rNote in rPart)
                        {
                            string resampler_exe = renderEngine.ResamplerPath;
                            List<string> args = rNote.Executors.GetResamplerArgs(true);
                            if (args.Count == 0) continue;
                            if (File.Exists(rNote.Executors.TempFilePath)) continue;
                            Process p = wine.CreateWineProcess(resampler_exe, args);
                            p.Start();
                            p.WaitForExit();
                            if (!resampleProcess.ContainsKey(rNote.Executors.TempFilePath))
                            {
                                resampleProcess.Add(rNote.Executors.TempFilePath, p);
                            }
                        }
                    }

                    //wavtool
                    if (WineHelper.UnderWine)
                    {
                        StringBuilder wavtoolBatContent = new StringBuilder();
                        string wavtoolBatFile = Path.Combine(WorkDir, "wavtool.bat");
                        wavtoolBatContent.AppendLine("chcp 932");//CodePage:Shift-JIS
                        foreach (URenderNote rNote in rPart)
                        {
                            string wavtool_exe = renderEngine.WavtoolPath;
                            List<string> args = rNote.Executors.GetWavtoolArgs(OutputFile, true);
                            wavtoolBatContent.AppendLine(TaskHelper.CreateCommandLine(wavtool_exe, args, WorkDir));
                        }
                        File.WriteAllText(wavtoolBatFile, wavtoolBatContent.ToString(), CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS"));
                        if (File.Exists(wavtoolBatFile))
                        {
                            Process wavP = wine.CreateWineProcess("cmd.exe", new List<string>(["/c", wavtoolBatFile]));
                            wavP.Start();
                            wavP.WaitForExit();
                            File.Delete(wavtoolBatFile);
                        }
                    }
                    else
                    {
                        foreach (URenderNote rNote in rPart)
                        {
                            string wavtool_exe = renderEngine.WavtoolPath;
                            List<string> args = rNote.Executors.GetWavtoolArgs(OutputFile, true);
                            Process p = wine.CreateWineProcess(wavtool_exe, args, WorkDir);
                            p.Start();
                            p.WaitForExit();
                        }
                    }

                    TaskHelper.FinishWavTool(OutputFile, WorkDir);
                }
            }
            FileLocker.UnlockFile(OutputFile);

            if (mCacnel) { return; }
            if (File.Exists(OutputFile))
            {
                TaskHelper.WaitForFileRelease(OutputFile);
                try
                {
                    var reader = new WaveFileReader(OutputFile);
                    reader.Close();
                }
                catch { try { File.Delete(OutputFile); } catch {; } Error?.Invoke("Rendered File Cannot Readable"); return; }
                if (mCacnel) { return; }

                if (loop == RenderPart.FirstTrack && bHaveXTrack)
                {
                    StartRenderTask(RenderPart.SecondTrack, OutputFile);
                    return;
                }
                CompleteTask(OutputFile, pitchtable, rPart, firstTrackAudioFile);
            }
        }

        private List<List<Point>> FormatPitchLines(SortedDictionary<double, double> pitchLines, double startMillSecond = 0)
        {
            List<List<Point>> pitLines = new List<List<Point>>();
            List<Point> pPLine = new List<Point>();
            foreach (var kv in pitchLines)
            {
                double PointX = (synthesisData.StartTime() - UtauProject.HeadPreSequenceMillsectionTime / 1000.0) + (kv.Key / 1000.0);
                if(PointX< startMillSecond/1000.0) continue; 
                pPLine.Add(new Point() { X = PointX, Y = kv.Value });
            }
            pitLines.Add(pPLine);
            return pitLines;
        }

        private void CompleteTask(string OutputWav, SortedDictionary<double, double> pitchLines, List<URenderNote> renderNotes,string? firstTrackOutputWav=null)
        {
            var audioInfo=TaskHelper.ReadWaveAudioData(OutputWav,synthesisData.StartTime());
            if (mCacnel) { return; }

            Dictionary<ISynthesisNote, SynthesizedPhoneme[]>? phonemeInfoList = UtauProject.GetPhonemeInfoList(renderNotes);
            //var pitLines= TaskHelper.WaveAudioDataPitchDetect2(audioInfo);
            var pitLines = FormatPitchLines(pitchLines, audioInfo.audio_StartMillsec);

            if(firstTrackOutputWav!=null && File.Exists(firstTrackOutputWav))
            {
                var xAudioInfo = TaskHelper.ReadWaveAudioData(firstTrackOutputWav, synthesisData.StartTime());
                audioInfo = AudioEffect.AudioEffectHelper.MixSynthesis(synthesisData, xAudioInfo, audioInfo);
            }

            AudioEffect.AudioEffectHelper.DoProcess(synthesisData, ref audioInfo);

            var ret = new SynthesisResult(audioInfo.audio_StartMillsec/1000.0, 44100, audioInfo.audio_Data, pitLines,phonemeInfoList);

            Complete?.Invoke(ret);
        }

        public void Stop()
        {
            //mCacnel = true;
            return;
        }

        public void Suspend()
        {
            return;
        }
    }
}
