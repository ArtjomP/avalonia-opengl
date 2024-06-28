#nullable enable

using System;
using System.Drawing;
using System.Collections.Generic;

using CommunityToolkit.Diagnostics;

using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;

using Silk.NET.OpenGL;


namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.ColorfulVoronoiScene;

//https://glslsandbox.com/e#103855.0
internal sealed class ColorfulVoronoiScene : IOpenGlScene
{
    private readonly OpenGlSceneParameter _speed;
    private readonly OpenGlSceneParameter _lineWidth;
    private readonly OpenGlSceneParameter _innerGradientWidthParameter;
    private readonly OpenGlSceneParameter _outerGradientWidthParameter;
    private readonly OpenGlSceneParameter _timePulseRange;
    private readonly OpenGlSceneParameter _gradientPulseFrequency;

    public ColorfulVoronoiScene(GlVersion glVersion)
    {
        _speed = new OpenGlSceneParameter("Speed", 55);
        _lineWidth = new OpenGlSceneParameter("LineWidth", 20);
        _innerGradientWidthParameter = new OpenGlSceneParameter("InnerGradientWidth", 20);
        _outerGradientWidthParameter = new OpenGlSceneParameter("OuterGradientWidth", 20);
        _timePulseRange = new OpenGlSceneParameter("TimePulseRange", 5);
        _gradientPulseFrequency = new OpenGlSceneParameter("GradientPulseFrequency", 5);

        Parameters = new OpenGlSceneParameter[]
        {
            _speed,
            _lineWidth,
            _innerGradientWidthParameter,
            _outerGradientWidthParameter,
            _timePulseRange,
            _gradientPulseFrequency
        };
    }

    private GradientParameters _gradientParameters = new(
        direction: Direction.Backward,
        leftGradientWidth: 0,
        rightGradientWidth: 0);

    private Single _timeValue = 0f;

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ColorfulVoronoi;

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
            @"Utils/OpenGl/Scenes/ColorfulVoronoiScene/voronoi.vert",
            @"Utils/OpenGl/Scenes/ColorfulVoronoiScene/voronoi.frag");
    }

    public void DeInitialize(GlInterface gl)
    {
        _vbo?.Dispose();
        _vao?.Dispose();
        _shader?.Dispose();
    }

    private Random _random = new Random();

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        var silkGl = _gl ?? GL.GetApi(gl.GetProcAddress);
        silkGl.ClearColor(Color.Black);
        silkGl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        silkGl.Enable(EnableCap.DepthTest);

        var shader = _shader;
        if (shader is not null)
        {
            shader.Use();

            shader.SetUniform("resolution_x", (Single)width);
            shader.SetUniform("resolution_y", (Single)height);
            shader.SetUniform("line_width", _lineWidth.Value / 100f);

            UpdateGradientWidth();
            var innerGradientWidthValue = _gradientParameters.leftGradientWidth;
            shader.SetUniform("inner_gradient_width", innerGradientWidthValue);

            var outerGradientWidthValue = _gradientParameters.rightGradientWidth;
            shader.SetUniform("outer_gradient_width", outerGradientWidthValue);

            shader.SetUniform("time", _timeValue);

            var speed = _speed.Value / 1000f;
            var offset = _timePulseRange.Value == 0
                ? 0
                : _random.Next(-_timePulseRange.Value, _timePulseRange.Value + 1) / 100f;
            _timeValue += speed + offset;
        }

        silkGl.DrawArrays(
            mode: GLEnum.TriangleStrip,
            first: 0,
            count: 4);
    }

    private void UpdateGradientWidth()
    {
        _gradientParameters = OpenGlUtils.UpdateGradientWidth(
            gradientParameters: _gradientParameters,
            pulse: _gradientPulseFrequency,
            leftWidth: _innerGradientWidthParameter,
            rightWidth: _outerGradientWidthParameter);
    }
}