#nullable enable

using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class Lines3Scene : IOpenGlScene
{
    public Lines3Scene(GlVersion glVersion)
    {
        GlVersion = glVersion;
        Parameters = new OpenGlSceneParameter[]
        {
            new OpenGlSceneParameter("Lines count", 1),
        };
    }

    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters
    { 
        get; 
    }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.Lines3;

    private GlInterface? _gl;

    private Line[]? _lines = null;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        if (gl is not null)
        {
            var startPoint = new Vector3(-0.5f, -0.5f, 0);
            var endPoint = new Vector3(0.5f, 0.5f, 0);
            var lines = new Line[]
            {
                new Line(GlVersion, gl, startPoint, endPoint)
            };
            foreach (var line in lines)
            {
                line.SetColor(red: 1f, green: 0f, blue: 0f, alpha: 1f);
                line.SetMvp(Matrix4x4.Identity);
            }

            _lines = lines;
        }        
    }

    public void DeInitialize(GlInterface gl)
    {
        if (gl is not null)
        {
        }

        ClearLines();
    }

    private void ClearLines()
    {
        var lines = _lines;
        if (lines is not null)
        {
            foreach (var line in lines)
            {
                line.Dispose();
            }
        }
    }

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        gl.ClearColor(r: 0f, g: 0f, b: 0f, a: 1);
        var glExtras = _glExtras;
        if (glExtras is not null)
        {
            var lines = _lines;
            if (lines is not null)
            {
                foreach (var line in lines)
                {
                    line.Draw();
                }
            }
        }
    }
}
