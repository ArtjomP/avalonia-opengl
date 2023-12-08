#nullable enable

using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class Lines4Scene : IOpenGlScene
{
    private readonly OpenGlSceneParameter _lineWidth;

    public Lines4Scene(GlVersion glVersion)
    {
        GlVersion = glVersion;
        _lineWidth = new OpenGlSceneParameter("Line width", 2);
        Parameters = new OpenGlSceneParameter[]
        {
            _lineWidth,
        };
    }

    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.Lines4;

    private GlInterface? _gl;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        if (gl is not null)
        {           
        }        
    }

    public void DeInitialize(GlInterface gl)
    {
        if (gl is not null)
        {
        }
    }

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {

    }
}