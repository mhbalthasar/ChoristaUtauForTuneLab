using TuneLab.Extensions.Voices;

namespace ChoristaUtauLoader
{
    [VoiceEngine("ChoristaUtau-Mores")]
    public class EngineLoader_Mores : UMoresamplerForTuneLab.Moresample_Engine { }
    [VoiceEngine("ChoristaUtau-Doppe")]
    public class EngineLoader_Doppe : UDoppeltlerForTuneLab.Doppeltler_Engine{ }
}
