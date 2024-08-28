using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtaubaseForTuneLab;

namespace UMoresamplerForTuneLab
{
    internal class Moresample_Render(string enginePath) : IRenderEngine
    {
        public string ResamplerPath { get { return "E:\\UserDirectory\\Downloads\\MinLab\\MoreSampler\\moresampler.exe"; } }
        public string WavtoolPath { get {
                return
                    @"F:\F\G\VocalUtau\VocalUtau.DebugExampleFiles\UTAUKernel\wavtool.exe";
                    //"E:\\UserDirectory\\Downloads\\MinLab\\MoreSampler\\moresampler.exe"; 
            } }

        public string EngineUniqueString => "Utau_MoreSampler";

        public bool WindowsOnly => true;
    }
}
