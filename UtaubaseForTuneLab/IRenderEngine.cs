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
    public abstract class IRenderEngine
    {
        public abstract string ResamplerPath { get; }
        public abstract string WavtoolPath { get; }
        public virtual bool WindowsOnly { get; } = true;
        public virtual bool IsX64bit { get; } = false;
        public abstract string EngineUniqueString { get; }
        public virtual bool SupportUltraTempo { get; } = false;
        public virtual int GetQueueSize(int defaultQueueSize) { return defaultQueueSize; }
        public virtual bool IsMuteTailOffset { get; } = false;
        public virtual IReadOnlyOrderedMap<string, AutomationConfig> AutomationConfigs { get; } = new OrderedMap<string, AutomationConfig>();
        public virtual IReadOnlyOrderedMap<string, IPropertyConfig> PartProperties { get; } = new OrderedMap<string, IPropertyConfig>();
        public virtual IReadOnlyOrderedMap<string, IPropertyConfig> NoteProperties { get; } = new OrderedMap<string, IPropertyConfig>();
        public virtual string GetNoteFlags(ISynthesisData data, ISynthesisNote note, string baseFlag = "", bool isXTrackFlag = false) { return ""; }
        public virtual string GetTimeFlags(ISynthesisData data, double time, string baseFlag = "", double areaDuration = 0, bool isXTrackFlag = false) { return ""; }
    }
}
