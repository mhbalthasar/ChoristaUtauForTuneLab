using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab;

namespace UDoppeltlerForTuneLab
{
    public class Doppeltler_Engine : UtauEngine
    {

        public override bool InitRenderEngine(string enginePath, out string? error)
        {
            RenderEngine = new Doppeltler_Render(enginePath);
            error = "";
            return true;
        }
    }
}
