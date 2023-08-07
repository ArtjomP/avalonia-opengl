#nullable enable

using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class Lines4Scene : IOpenGlScene
{
    private OpenGlSceneParameter _lineWidth;

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

    private GlExtrasInterface? _glExtras;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        _glExtras ??= new GlExtrasInterface(gl);
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
        gl.ClearColor(r: 0f, g: 0f, b: 0f, a: 0.5f);
        var glExtras = _glExtras;
        if (glExtras is not null)
        {
            gl.LineWidth(_lineWidth.Value);
            gl.Begin(GL_LINES);
            gl.Color3f(0f, 1f, 0f);
            gl.Vertex2f(0.1f, 0.1f);
            gl.Vertex2f(0.9f, 0.9f);
            gl.End();
        }
    }
}
