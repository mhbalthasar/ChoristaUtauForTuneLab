using System.Numerics;

namespace ImGuiNET.SDL2CS
{

    /// <summary>
    /// Extended ImWindowBase with easy interface.
    /// </summary>
    public class ImWindowExt_CJK : ImWindowBase_CJK
    {
        private const string mTitle = "ImGuiNet.SdlCs ImWindowExt Instance";
        public const int mWidth = 1280;
        public const int mHeigt = 720;

        public enum Mode
        {
            Normal,
            Layout,
            Single
        };

        public readonly Mode mMode;

        /// <summary>
        /// Layout that will run in this window.
        /// You could add layout by OnLayoutUpdate, but that will not include in this windows's management.
        /// </summary>
        public LayoutUpdateMethod mAction;

        public ImGuiWindowFlags mFlags = ImGuiWindowFlags.None;

        public ImWindowExt_CJK(Mode mode)
            : this(mode, mTitle, null, mWidth, mHeigt, ImGuiWindowFlags.None)
        {
        }

        public ImWindowExt_CJK(Mode mode, string title)
            : this(mode, title, null, mWidth, mHeigt, ImGuiWindowFlags.None)
        {
        }

        public ImWindowExt_CJK(Mode mode, LayoutUpdateMethod action)
            : this(mode, mTitle, action, mWidth, mHeigt, ImGuiWindowFlags.None)
        {
        }

        public ImWindowExt_CJK(Mode mode, string title, LayoutUpdateMethod action)
            : this(mode, title, action, mWidth, mHeigt, ImGuiWindowFlags.None)
        {
        }

        public ImWindowExt_CJK(Mode mode, string title, ImGuiWindowFlags flags)
            : this(mode, title, null, mWidth, mHeigt, flags)
        {
        }

        /// <summary>
        /// ImWindow constructor.
        /// </summary>
        /// <param name="mode">Window mode</param>
        /// <param name="action">User UI method</param>
        /// <param name="title">Window caption</param>
        /// <param name="width">Window width</param>
        /// <param name="height">Window height</param>
        /// <param name="flags">Additional flags when mode is WindowMode.Single</param>
        public ImWindowExt_CJK(Mode mode, string title, LayoutUpdateMethod action, int width, int height, ImGuiWindowFlags flags)
            : base(title, width, height)
        {
            mMode = mode;
            mAction = action;
            mFlags = flags;

            OnLayoutUpdate += UpdateLayout;
        }

        private bool UpdateLayout()
        {
            // Overlay
            if (mMode == Mode.Layout)
            {
                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(Size);
                ImGui.SetNextWindowBgAlpha(0);
                ImGui.Begin("Overlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBringToFrontOnFocus);
                ImGui.Text(string.Format("FPS:{0:0.00}", ImGui.GetIO().Framerate));
                ImGui.SetCursorPos(new Vector2(0, 0));
            }
            else if (mMode == Mode.Single)
            {
                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(Size);
                ImGui.SetNextWindowBgAlpha(0);
                ImGui.Begin("Overlay", mFlags | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            }

            if (mAction != null && !mAction())
            {
                Exit();
            }

            if (mMode != Mode.Normal)
            {
                ImGui.End();
            }

            return true;
        }
    }
}