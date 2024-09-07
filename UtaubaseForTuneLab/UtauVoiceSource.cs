using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;
using ChoristaUtauApi.UPhonemizer;
using ChoristaUtauApi.UVoiceBank;

namespace UtaubaseForTuneLab
{
    internal class UtauVoiceSource(
    IRenderEngine renderEngine,
    VoiceBank voiceBank,
    IReadOnlyOrderedMap<string, AutomationConfig> automationConfigs,
    IReadOnlyOrderedMap<string, IPropertyConfig> partProperties,
    IReadOnlyOrderedMap<string, IPropertyConfig> noteProperties) : IVoiceSource
    {
        public string Name => renderEngine.EngineUniqueString + " : " + voiceBank.Name;

        public string DefaultLyric => voiceBank.DefaultLyric;

        public IReadOnlyOrderedMap<string, AutomationConfig> AutomationConfigs => automationConfigs;

        public IReadOnlyOrderedMap<string, IPropertyConfig> PartProperties => partProperties;

        public IReadOnlyOrderedMap<string, IPropertyConfig> NoteProperties => noteProperties;

        public ISynthesisTask CreateSynthesisTask(ISynthesisData data)
        {
            return new UtauSynthesisTask(data, renderEngine, voiceBank);
        }

        public IReadOnlyList<SynthesisSegment<T>> Segment<T>(SynthesisSegment<T> segment) where T : ISynthesisNote
        {
            return this.SimpleSegment(segment, segment.PartProperties.GetDouble(UtauEngine.MinSegmentSpacingID, UtauEngine.MinSegmentSpacingConfig.DefaultValue));
        }

    }
}
