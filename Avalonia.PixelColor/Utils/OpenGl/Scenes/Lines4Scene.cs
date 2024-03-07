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
        const Int32 GL_PROJECTION = 0;
        const Int32 GL_MODELVIEW = 1;
        const Int32 GL_LINES = 2;
        if (gl is not null)
        {
            gl.MatrixMode(GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho(0, width, height, 0, -1f, 1f);

            gl.MatrixMode(GL_MODELVIEW);
            gl.LoadIdentity();

            gl.ClearColor(r: 0.3f, g: 0.3f, b: 0.3f, a: 1f);

            gl.Color3f(1f, 1f, 1f);
            gl.LineWidth(_lineWidth.Value);
            gl.Begin(GL_LINES);
            gl.Vertex2f(-10f, -10f);
            gl.Vertex2f(10f, 10f);
            gl.End();
        }
    }
}