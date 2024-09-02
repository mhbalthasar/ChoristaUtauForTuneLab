using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;

namespace UtaubaseForTuneLab
{
    public interface IRenderEngine
    {
        public string ResamplerPath { get; }
        public string WavtoolPath { get; }
        public bool WindowsOnly { get; }
        public string EngineUniqueString { get; }


        public IReadOnlyOrderedMap<string, AutomationConfig> AutomationConfigs { get; }
        public IReadOnlyOrderedMap<string, IPropertyConfig> PartProperties { get; }
        public IReadOnlyOrderedMap<string, IPropertyConfig> NoteProperties { get; }
        public string GetNoteFlags(ISynthesisData data, ISynthesisNote note, string baseFlag = "", bool isXTrackFlag=false);
        public string GetTimeFlags(ISynthesisData data, double time, string baseFlag = "", double areaDuration = 0, bool isXTrackFlag = false);
    }
}
