using Avalonia;
using System;
using System.Collections.Generic;
using Avalonia.OpenGL;

namespace Avalonia.PixelColor
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions
                {
                    EnableMultiTouch = true,
                    UseDBusMenu = true,
                    RenderingMode = new List<X11RenderingMode>
                    {
                        X11RenderingMode.Glx,
                        X11RenderingMode.Software
                    },
                    GlProfiles = new List<GlVersion>
                    {
                        new GlVersion(GlProfileType.OpenGL, 4, 0)
                    },
                })
                .With(new Win32PlatformOptions
                {
                    RenderingMode = new List<Win32RenderingMode>
                    {
#if !DEBUG
                    Win32RenderingMode.Wgl,
#endif
                        Win32RenderingMode.Software,
                        Win32RenderingMode.AngleEgl,
                    },
                    WglProfiles = new List<GlVersion>
                    {
                        new GlVersion(GlProfileType.OpenGL, 4, 0)
                    },
                    ShouldRenderOnUIThread = false,
                })
                .With(new MacOSPlatformOptions
                {
                    ShowInDock = true,
                })
                .LogToTrace();
    }
}