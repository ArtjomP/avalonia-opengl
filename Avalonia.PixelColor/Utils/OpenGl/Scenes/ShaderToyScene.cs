using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using Common;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using CommunityToolkit.Diagnostics;
using Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

public sealed class ShaderToyScene : IOpenGlScene
{
    private readonly String _vertexShaderSource = @"//Here we specify the version of our shader.
#version 400 core
layout (location = 0) in vec4 VERTEXDATA;

uniform float shift;

uniform vec2 RENDERSIZE;
out vec2 fragCoord;
out vec4 isf_Position;

void main()
{
    mat4 projectionMatrix = mat4(2./RENDERSIZE.x, 0., 0., -1.,
                                 0., 2./RENDERSIZE.y, 0., -1.,
                                 0., 0., -1., 0.,
                                 0., 0., 0., 1.);
    gl_Position = VERTEXDATA;// * projectionMatrix;
    isf_Position = gl_Position;
    fragCoord = vec2((gl_Position.x+1.0)/2.0, (gl_Position.y+1.0)/2.0);
}";

    private readonly String _fragmentShaderSource;

    private readonly IShaderToyFragmentShaderConverter _shaderToyFragmentShaderConverter;

    public ShaderToyScene(
        GlVersion glVersion,
        String fragmentShaderSource)
    {
        _shaderToyFragmentShaderConverter = new ShaderToyFragmentShaderConverter();
        Guard.IsNotNullOrEmpty(fragmentShaderSource);
        _fragmentShaderSource = _shaderToyFragmentShaderConverter
            .ConvertToOpenGlFragmentShader(fragmentShaderSource);

        Parameters = Enumerable.Empty<OpenGlSceneParameter>();
    }

    private GL? _gl;

    private BufferObject<Single>? _vbo;

    private VertexArrayObject<Single, UInt32>? _vao;

    private Silk.Shader? _shader;

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ShaderToy;

    public void DeInitialize(GlInterface gl)
    {
        _vbo?.Dispose();
        _vao?.Dispose();
        _shader?.Dispose();
    }

    public void Initialize(GlInterface gl)
    {
        Guard.IsNotNull(gl);

        _gl = GL.GetApi(gl.GetProcAddress);
        _vbo = new BufferObject<Single>(
            gl: _gl,
            data: OpenGlConstants.VerticesFullScene,
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
            gl: _gl,
            vertexPath: _vertexShaderSource,
            fragmentPath: _fragmentShaderSource,
            loadShadersFromFile: false);
    }

    private Single _timeValue = 0f;

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        var silkGl = _gl ?? GL.GetApi(gl.GetProcAddress);
        silkGl.ClearColor(System.Drawing.Color.Black);
        silkGl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        silkGl.Enable(EnableCap.DepthTest);
        var shader = _shader;
        if (shader is not null)
        {
            shader.Use();
            shader.SetUniform(name: "iResolution", value: new Vector3(x: width, y: height, z: 1));
            shader.SetUniform(name: "iTime", value: _timeValue);
            shader.SetUniform(name: "iForce", value: _timeValue);
        }

        _timeValue += 0.01f;
        silkGl.DrawArrays(
            mode: PrimitiveType.TriangleStrip,
            first: 0,
            count: 4);
    }
}