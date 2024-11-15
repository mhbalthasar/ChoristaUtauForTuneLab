using System;
using System.Numerics;
using SDL2;

namespace ImGuiNET.SDL2CS
{

    /// <summary>
    /// Basic window of ImGuiNET.
    /// </summary>
    public class ImWindowBase_CJK : SDL2Window_CJK
    {
        public static readonly Vector4 defaultClearColor = new Vector4(0.4f, 0.5f, 0.6f, 1.0f);

        private double g_Time = 0.0f;
        private int g_FontTexture = 0;

        public Vector4 BackgroundColor
        {
            get;
            set;
        } = defaultClearColor;

        public delegate bool LayoutUpdateMethod();

        /// <summary>
        /// User layout update method.
        /// </summary>
        public event LayoutUpdateMethod OnLayoutUpdate;

        public delegate void WindowStartMethod(ImWindowBase_CJK window);
        public event WindowStartMethod OnWindowStart;

        public delegate void WindowExitMethod(ImWindowBase_CJK window);
        public event WindowExitMethod OnWindowExit;

        public ImWindowBase_CJK(string title = "ImGuiNET.SdlCs Basic Window", int width = 1280, int height = 760,
            SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) : base(title, width, height, flags)
        {
            ImGui.CreateContext();
            ImWindowHelper.Initialize();

            base.OnStart += OnStartHander;
            base.OnLoop += OnLoopHandler;
            base.OnEvent += OnEventHander;
            base.OnExit += OnExitHandler;
        }

        private void OnStartHander(SDL2Window_CJK window)
        {
            //if (!File.Exists("imgui.ini"))
            //    File.WriteAllText("imgui.ini", "");
            Create();
            OnWindowStart?.Invoke(this);
        }

        private void OnLoopHandler(SDL2Window_CJK window)
        {
            GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, BackgroundColor.W);
            GL.Clear(GL.GlEnum.GL_COLOR_BUFFER_BIT);
            Render();
            Swap();
        }

        private void OnEventHander(SDL2Window_CJK window, SDL.SDL_Event e)
        {
            var io = ImGui.GetIO();
            if (e.type == SDL.SDL_EventType.SDL_TEXTINPUT)
            {
                unsafe
                {
                    io.AddInputCharactersUTF8(SDL.UTF8_ToManaged((nint)e.text.text));
                }
                return;
            }
            ImWindowHelper.EventHandler(e);
        }
        private void OnExitHandler(SDL2Window_CJK window)
        {
            OnWindowExit?.Invoke(this);
        }

        private void Render()
        {
            ImWindowHelper.NewFrame(Size, Vector2.One, ref g_Time);
            UpdateLayout();
            ImWindowHelper.Render(Size);
        }

        private unsafe void Create()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build texture atlas
            //ImFontTextureData texData = ;
            io.Fonts.GetTexDataAsAlpha8(out byte* pixels, out int width, out int height);

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            int lastTexture;
            GL.GetIntegerv(GL.GlEnum.GL_TEXTURE_BINDING_2D, out lastTexture);

            // Create OpenGL texture
            GL.GenTextures(1, out g_FontTexture);
            GL.BindTexture(GL.GlEnum.GL_TEXTURE_2D, g_FontTexture);
            GL.TexParameteri(GL.GlEnum.GL_TEXTURE_2D, GL.GlEnum.GL_TEXTURE_MIN_FILTER, (int)GL.GlEnum.GL_LINEAR);
            GL.TexParameteri(GL.GlEnum.GL_TEXTURE_2D, GL.GlEnum.GL_TEXTURE_MAG_FILTER, (int)GL.GlEnum.GL_LINEAR);
            GL.PixelStorei(GL.GlEnum.GL_UNPACK_ROW_LENGTH, 0);
            GL.TexImage2D(
                GL.GlEnum.GL_TEXTURE_2D,
                0,
                (int)GL.GlEnum.GL_ALPHA,
                width,
                height,
                0,
                GL.GlEnum.GL_ALPHA,
                GL.GlEnum.GL_UNSIGNED_BYTE,
                (IntPtr)pixels
            );

            // Store the texture identifier in the ImFontAtlas substructure.
            io.Fonts.SetTexID((IntPtr)g_FontTexture);
            io.Fonts.ClearTexData(); // Clears CPU side texture data.
            GL.BindTexture(GL.GlEnum.GL_TEXTURE_2D, lastTexture);
        }

        private void UpdateLayout()
        {
            if (OnLayoutUpdate == null)
            {
                ImGui.Text($"Create a new class inheriting {GetType().FullName}, overriding {nameof(UpdateLayout)}!");
            }
            else
            {
                foreach (LayoutUpdateMethod del in OnLayoutUpdate.GetInvocationList())
                {
                    if (!del())
                    {
                        OnLayoutUpdate -= del;
                    }
                }
            }
        }

        public override void Dispose(bool disposing)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (disposing)
            {
                // Dispose managed state (managed objects).
            }

            // Free unmanaged resources (unmanaged objects) and override a finalizer below.
            // Set large fields to null.
            if (g_FontTexture != 0)
            {
                // Texture gets deleted with the context.
                // GL.DeleteTexture(g_FontTexture);
                if ((int)io.Fonts.TexID == g_FontTexture)
                    io.Fonts.TexID = IntPtr.Zero;
                g_FontTexture = 0;
            }

            base.Dispose(disposing);
        }

        ~ImWindowBase_CJK()
        {
            Dispose(false);
        }
    }
}