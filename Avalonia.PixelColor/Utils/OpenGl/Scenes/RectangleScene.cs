#nullable enable

using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using Common;
using CommunityToolkit.Diagnostics;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class RectangleScene : IOpenGlScene
{
    public IEnumerable<OpenGlSceneParameter> Parameters => Array.Empty<OpenGlSceneParameter>();

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.Rectangle;

    private GL? _gl;

    private BufferObject<Single>? _vbo;

    private VertexArrayObject<Single, UInt32>? _vao;

    private Silk.Shader? _shader;

    private static readonly Single[] _vertices =
    {
            //X    Y      Z 
            -1.0f, -1.0f, 0.0f,
             1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f
    };

    public unsafe void Initialize(GlInterface gl)
    {
        Guard.IsNotNull(gl);

        _gl = GL.GetApi(gl.GetProcAddress);
        _vbo = new BufferObject<Single>(
            gl: _gl,
            data: _vertices,
            bufferType: BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<Single, UInt32>(
            gl: _gl,
            vbo: _vbo);
        _vao.VertexAttributePointer(
            index: 0,
            count: 3,
            type: VertexAttribPointerType.Float,
            vertexSize: 3,
            offSet: 0);
        _shader = new Silk.Shader(
            _gl,
            ConstantStrings.VertexShader,
            ConstantStrings.FragmentShader,
            false);
    }

    public void DeInitialize(GlInterface gl)
    {
        _vbo?.Dispose();
        _vao?.Dispose();
        _shader?.Dispose();
    }

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        var silkGl = _gl ?? GL.GetApi(gl.GetProcAddress);
        silkGl.ClearColor(Color.Black);
        silkGl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        silkGl.Enable(EnableCap.DepthTest);

        var shader = _shader;
        shader?.Use();

        silkGl.DrawArrays(
            mode: PrimitiveType.TriangleStrip,
            first: 0,
            count: 4);
    }
}
