#nullable enable

using Avalonia;
using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using CommunityToolkit.Diagnostics;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.LinesSilkScene;

public sealed class LinesSilkScene : IOpenGlScene
{
    private GL _gl;
    private BufferObject<float> _vbo;
    private VertexArrayObject<float, uint> _vao;
    private Silk.Shader _shader;

    private static readonly Single[] _vertices =
    {
            //X    Y      Z 
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 0.0f
        };

    public IEnumerable<OpenGlSceneParameter> Parameters
        => Enumerable.Empty<OpenGlSceneParameter>();

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.LinesSilk;

    public void DeInitialize(GlInterface gl)
    {
        _vbo.Dispose();
        _vao.Dispose();
        _shader.Dispose();
    }

    public void Initialize(GlInterface gl)
    {
        Guard.IsNotNull(gl);

        _gl = GL.GetApi(gl.GetProcAddress);
        _vbo = new BufferObject<float>(
            gl: _gl,
            data: _vertices,
            bufferType: BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(
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
            @"Utils\OpenGl\Scenes\LinesSilkScene\lines.vert",
            @"Utils\OpenGl\Scenes\LinesSilkScene\lines.frag");
    }

    public void Render(GlInterface gl, int width, int height)
    {
        _gl.ClearColor(Color.Black);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Viewport(
            x: 0,
            y: 0,
            width: Convert.ToUInt32(width),
            height: Convert.ToUInt32(height));
        _shader.Use();
        _shader.SetUniform("RENDERSIZE", new Vector2((Single)width, (Single)height));
        _shader.SetUniform("shift", (Single)DateTime.Now.Millisecond % 1000 / 1000f);
        _shader.SetUniform("angle", 0.5f);
        _shader.SetUniform("line_width", (Single)Math.Cos(DateTime.Now.Millisecond / 1000f) * 0.5f);
        _shader.SetUniform("spacing", 0.5f);
        _shader.SetUniform("color1", new Vector4(0.0f, 1.0f, 0.5f, 1.0f));
        _shader.SetUniform("color2", new Vector4(0.3f, 0.0f, 0.3f, 1.0f));

        _gl.DrawArrays(
            mode: PrimitiveType.TriangleStrip,
            first: 0,
            count: 4);
    }
}