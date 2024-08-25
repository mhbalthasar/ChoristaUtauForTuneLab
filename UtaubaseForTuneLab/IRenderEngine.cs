using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab
{
    public interface IRenderEngine
    {
        public string ResamplerPath { get; }
        public string WavtoolPath { get; }
        public bool WindowsOnly { get; }
        public string EngineUniqueString { get; }
    }
}
