#nullable enable

using Avalonia.OpenGL;
using System.Collections.Generic;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public interface IOpenGlScene
{
    void Initialize(GlInterface gl);

    void DeInitialize(GlInterface gl);

    void Render(
        GlInterface gl,
        Int32 width,
        Int32 height);

    IEnumerable<OpenGlSceneParameter> Parameters { get; }

    OpenGlScenesEnum Scene { get; }
}
