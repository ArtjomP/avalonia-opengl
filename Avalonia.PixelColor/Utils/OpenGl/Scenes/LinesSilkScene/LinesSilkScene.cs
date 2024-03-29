﻿#nullable enable

using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using CommunityToolkit.Diagnostics;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.LinesSilkScene;

public sealed class LinesSilkScene : IOpenGlScene
{
    private readonly OpenGlSceneParameter _lineWidth;
    private readonly OpenGlSceneParameter _leftGradientWidthParameter;
    private readonly OpenGlSceneParameter _rightGradientWidthParameter;
    private readonly OpenGlSceneParameter _pulseFrequency;
    private readonly OpenGlSceneParameter _r1;
    private readonly OpenGlSceneParameter _g1;
    private readonly OpenGlSceneParameter _b1;
    private readonly OpenGlSceneParameter _r2;
    private readonly OpenGlSceneParameter _g2;
    private readonly OpenGlSceneParameter _b2;
    private readonly OpenGlSceneParameter _angle;
    private readonly OpenGlSceneParameter _speed;
    private readonly OpenGlSceneParameter _spacing;

    public LinesSilkScene()
    {
        _lineWidth = new OpenGlSceneParameter("Line width", 10);
        _leftGradientWidthParameter = new OpenGlSceneParameter("Left gradient width", 30);
        _rightGradientWidthParameter = new OpenGlSceneParameter("Right gradient width", 30);
        _pulseFrequency = new OpenGlSceneParameter("Pulse frequency", 10);
        _r1 = new OpenGlSceneParameter("R1", Byte.MaxValue);
        _g1 = new OpenGlSceneParameter("G1", Byte.MaxValue);
        _b1 = new OpenGlSceneParameter("B1", Byte.MaxValue);
        _r2 = new OpenGlSceneParameter("R2", Byte.MinValue);
        _g2 = new OpenGlSceneParameter("G2", Byte.MinValue);
        _b2 = new OpenGlSceneParameter("B2", Byte.MinValue);
        _angle = new OpenGlSceneParameter("Angle", 64);
        _speed = new OpenGlSceneParameter("Speed", 0);
        _spacing = new OpenGlSceneParameter("Spacing", 180);
        Parameters = new OpenGlSceneParameter[]
        {
            _lineWidth,
            _leftGradientWidthParameter,
            _rightGradientWidthParameter,
            _pulseFrequency,
            _angle,
            _speed,
            _spacing,
            _r1,
            _g1,
            _b1,
            _r2,
            _g2,
            _b2,
        };
    }

    private GL? _gl;

    private BufferObject<Single>? _vbo;

    private VertexArrayObject<Single, UInt32>? _vao;

    private Silk.Shader? _shader;

    private GradientParameters _gradientParameters = new GradientParameters(
        direction: Direction.Backward, 
        leftGradientWidth: 0,
        rightGradientWidth: 0);

    private Single shift = 0;

    private static readonly Single[] _vertices =
    {
            //X    Y      Z 
            -1.0f, -1.0f, 0.0f,
             1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f
    };

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.LinesSilk;

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
            @"Utils/OpenGl/Scenes/LinesSilkScene/lines.vert",
            @"Utils/OpenGl/Scenes/LinesSilkScene/lines.frag");
    }

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
            shader.SetUniform("RENDERSIZE", new Vector2((Single)width, (Single)height));
            var speed = (Single)_speed.Value / Byte.MaxValue;
            shift += speed / 10.0f;
            shader.SetUniform("shift", shift);
            var angle = (Single)_angle.Value / (Single)(Byte.MaxValue * 0.5);
            shader.SetUniform("angle", angle);

            var lineWidth = (Single)_lineWidth.Value / Byte.MaxValue;
            shader.SetUniform("line_width", lineWidth);

            UpdateGradientWidth();
            shader.SetUniform("left_gradient_width", _gradientParameters.leftGradientWidth);
            shader.SetUniform("right_gradient_width", _gradientParameters.rightGradientWidth);

            var spacing = (Single)_spacing.Value / Byte.MaxValue;
            shader.SetUniform("spacing", spacing);
            var r1 = (Single)_r1.Value / Byte.MaxValue;
            var g1 = (Single)_g1.Value / Byte.MaxValue;
            var b1 = (Single)_b1.Value / Byte.MaxValue;
            var r2 = (Single)_r2.Value / Byte.MaxValue;
            var g2 = (Single)_g2.Value / Byte.MaxValue;
            var b2 = (Single)_b2.Value / Byte.MaxValue;
            shader.SetUniform("color1", new Vector4(r1, g1, b1, 1.0f));
            shader.SetUniform("color2", new Vector4(r2, g2, b2, 1.0f));
        }

        silkGl.DrawArrays(
            mode: PrimitiveType.TriangleStrip,
            first: 0,
            count: 4);
    }

    private void UpdateGradientWidth()
    {
        _gradientParameters = OpenGlUtils.UpdateGradientWidth(
            gradientParameters: _gradientParameters,
            pulse: _pulseFrequency,
            leftWidth: _leftGradientWidthParameter,
            rightWidth: _rightGradientWidthParameter);
    }
}