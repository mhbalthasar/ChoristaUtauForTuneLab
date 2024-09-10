using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab;

namespace UMoresamplerForTuneLab
{
    [VoiceEngine("ChoristaUtau-Mores")]
    public class Moresample_Engine : UtauEngine
    {

        public override bool InitRenderEngine(string enginePath, out string? error)
        {
            RenderEngine = new Moresample_Render(enginePath);
            error = "";
            return true;
        }
    }
}
