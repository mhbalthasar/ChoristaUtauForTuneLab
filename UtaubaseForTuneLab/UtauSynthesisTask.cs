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
            
            Task.Run(() =>
            {
                UTaskProject uTask = UtauProject.GenerateFrom(synthesisData,voiceBank).ProcessPhonemizer(phonemizer);
                List<URenderNote> rPart = uTask.Part.GenerateRendPart(renderEngine.EngineUniqueString);

                //set pitchs
                var pitchtable = UtauProject.PitchPrerender(synthesisData);
                foreach (URenderNote rNote in rPart)
                {
                    rNote.SetPitchBends((ms_times) =>
                    {
                        double[] ret=new double[ms_times.Length];
                        for(int i=0;i< ms_times.Length;i++)
                        {
                            ret[i] = (double)pitchtable[(int)ms_times[i]];
                        }
                        return ret;
                    });
                }



                //resampler
                foreach (URenderNote rNote in rPart)
                {
                    string resampler_exe = renderEngine.ResamplerPath;
                    List<string> args=rNote.GetResamplerArgs();
                    if (args.Count == 0) continue;
                    if (File.Exists(rNote.TempFilePath)) continue;
                    Process p = new Process();
                    p.StartInfo.FileName = resampler_exe;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    foreach (string arg in args) { p.StartInfo.ArgumentList.Add(arg); };
                    p.Start();
                    p.WaitForExit();
                    string errors=p.StandardError.ReadToEnd();
                    string outputs=p.StandardOutput.ReadToEnd();

                    string k = "";
                }

                //wavtool
                string OutputFile = "C:\\Users\\balthasar\\AppData\\Local\\Temp\\UtauSharp\\temp.wav";
                string WorkDir = "";
                {//输出temp.bat
                    string ParentDir=Path.GetDirectoryName(OutputFile);
                    string HelpName=Path.GetFileNameWithoutExtension(OutputFile);
                    WorkDir=Path.Combine(ParentDir, string.Format("{0}_conf",HelpName));
                    if(!Directory.Exists(WorkDir)) { Directory.CreateDirectory(WorkDir); }
                    string TempFile = Path.Combine(WorkDir, "temp.bat");
                    using (FileStream fs = new FileStream(TempFile,FileMode.Create,FileAccess.ReadWrite))
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
                }
                //YouMustSetup Temp.bat For Sync
                foreach (URenderNote rNote in rPart)
                {
                    string wavtool_exe = renderEngine.WavtoolPath;
                    List<string> args = rNote.GetWavToolArgs(OutputFile);
                    Process p = new Process();
                    p.StartInfo.FileName = wavtool_exe;
                    p.StartInfo.WorkingDirectory = WorkDir;//Moresampler的wavtool运行需要在WorkingDirectory中具备Temp.bat
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    foreach (string arg in args) { p.StartInfo.ArgumentList.Add(arg); };
                    p.Start();
                    p.WaitForExit();
                    string errors = p.StandardError.ReadToEnd();
                    string outputs = p.StandardOutput.ReadToEnd();

                    string k = "";
                }
            });
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
