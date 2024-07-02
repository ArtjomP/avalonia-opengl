using Avalonia.OpenGL;
using System;

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
                .With(new Win32PlatformOptions
                {
                    RenderingMode =
                    [
// #if !DEBUG
                        Win32RenderingMode.Wgl,
// #endif
                        Win32RenderingMode.Software,
                        Win32RenderingMode.AngleEgl,
                    ],
                    WglProfiles =
                    [
                        new GlVersion(GlProfileType.OpenGL, 3, 3)
                    ],
                })
                .LogToTrace();
    }
}