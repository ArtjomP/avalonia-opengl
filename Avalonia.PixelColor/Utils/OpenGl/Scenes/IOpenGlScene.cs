#nullable enable

using Avalonia.OpenGL;
using System.Collections.Generic;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

public interface IOpenGlScene
{
    void Initialize(GlInterface gl);

    void DeInitialize(GlInterface gl);

    void Render(
        GlInterface gl,
        int width,
        int height);

    IEnumerable<OpenGlSceneParameter> Parameters { get; }

    OpenGlScenesEnum Scene { get; }
}
