using SDL2;
using System;
using System.Numerics;

namespace ImGuiNET.SDL2CS
{

    public class SDL2Window_CJK : IDisposable
    {

        public IntPtr Window
        {
            get;
            private set;
        }

        public IntPtr Context
        {
            get;
            private set;
        }

        public float MaxFPS { get; set; } = 60.0f;

        public string Title
        {
            get
            {
                return SDL.SDL_GetWindowTitle(Window);
            }
            set
            {
                SDL.SDL_SetWindowTitle(Window, value);
            }
        }

        public Vector2 Position
        {
            get
            {
                int x, y;
                SDL.SDL_GetWindowPosition(Window, out x, out y);
                return new Vector2(x, y);
            }
            set
            {
                SDL.SDL_SetWindowPosition(Window, (int)Math.Round(value.X), (int)Math.Round(value.Y));
            }
        }

        public Vector2 Size
        {
            get
            {
                int x, y;
                SDL.SDL_GetWindowSize(Window, out x, out y);
                return new Vector2(x, y);
            }
            set
            {
                SDL.SDL_SetWindowSize(Window, (int)Math.Round(value.X), (int)Math.Round(value.Y));
            }
        }

        public SDL.SDL_WindowFlags Flags => (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(Window);

        public bool IsAlive
        {
            get;
            private set;
        } = false;

        public bool IsVisible => (Flags & SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN) == 0;

        internal Action<SDL2Window_CJK> OnStart;

        internal Action<SDL2Window_CJK> OnLoop;

        internal Action<SDL2Window_CJK> OnExit;

        /// <summary>
        /// Event handler.
        /// Return false mean masking default hander.
        /// </summary>
        public Action<SDL2Window_CJK, SDL.SDL_Event> OnEvent;

        internal SDL2Window_CJK(string title, int width, int height, SDL.SDL_WindowFlags flags)
        {
            SDL2Helper.Initialize();
            if (Window != IntPtr.Zero)
                throw new InvalidOperationException("SDL2Window already initialized, Dispose() first before reusing!");
            Window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, flags);
            SDL.SDL_StartTextInput();
            if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL) == SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL)
            {
                Context = SDL.SDL_GL_CreateContext(Window);
                SDL.SDL_GL_MakeCurrent(Window, Context);
                SDL.SDL_GL_SetSwapInterval(1);  // Enable vsync
            }
        }

        public void Show()
        {
            SDL.SDL_ShowWindow(Window);
        }

        public void Hide()
        {
            SDL.SDL_HideWindow(Window);
        }

        public void Run()
        {
            Show();
            IsAlive = true;
            OnStart?.Invoke(this);
            while (IsAlive)
            {
                //PollEvents
                SDL.SDL_Event e;
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        Exit();
                    OnEvent?.Invoke(this, e);
                }

                OnLoop?.Invoke(this);
            }
            OnExit?.Invoke(this);
        }

        public void Swap()
        {
            SDL.SDL_GL_SwapWindow(Window);
        }

        public void Exit()
        {
            IsAlive = false;
        }

        ~SDL2Window_CJK()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects).
            }

            // Free unmanaged resources (unmanaged objects) and override a finalizer below.
            // Set large fields to null.

            if (Window != IntPtr.Zero)
            {
                SDL.SDL_StopTextInput();
                SDL.SDL_DestroyWindow(Window);
                Window = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}