﻿#nullable enable

using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using CommunityToolkit.Diagnostics;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.LinesSilkScene;

public sealed class LinesSilkScene : IOpenGlScene
{
    private readonly OpenGlSceneParameter _lineWidth;
    private readonly OpenGlSceneParameter _gradientWidth;
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
        _lineWidth = new OpenGlSceneParameter("Line width", 10, 0, 40);
        _gradientWidth = new OpenGlSceneParameter("Gradient width", 30, 0, 50);
        _pulseFrequency = new OpenGlSceneParameter("Pulse frequency", Byte.MaxValue);
        _r1 = new OpenGlSceneParameter("R1", Byte.MaxValue);
        _g1 = new OpenGlSceneParameter("G1", Byte.MaxValue);
        _b1 = new OpenGlSceneParameter("B1", Byte.MaxValue);
        _r2 = new OpenGlSceneParameter("R2", Byte.MinValue);
        _g2 = new OpenGlSceneParameter("G2", Byte.MinValue);
        _b2 = new OpenGlSceneParameter("B2", Byte.MinValue);
        _angle = new OpenGlSceneParameter("Angle", 128);
        _speed = new OpenGlSceneParameter("Speed", 0);
        _spacing = new OpenGlSceneParameter("Spacing", 150, 90, Byte.MaxValue);
        Parameters = new OpenGlSceneParameter[]
        {
            _lineWidth,
            _gradientWidth,
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

        gradientWidth = (float)_gradientWidth.Value / Byte.MaxValue;
    }

    private GL? _gl;
    private BufferObject<Single>? _vbo;
    private VertexArrayObject<Single, UInt32>? _vao;
    private Silk.Shader? _shader;

    private float gradientWidth;
    private PulseDirection CurrentPulseDirection = PulseDirection.Backward;

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
            var shift = (Single)(DateTime.Now.Millisecond % 1000) / 1000f * speed * 10;
            shader.SetUniform("shift", shift);
            var angle = (Single)_angle.Value / Byte.MaxValue;
            shader.SetUniform("angle", angle);

            var lineWidth = (Single)_lineWidth.Value / Byte.MaxValue;
            shader.SetUniform("line_width", lineWidth);

            updateGradientWidth();
            shader.SetUniform("gradient_width", gradientWidth);

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

    private void updateGradientWidth()
    {
        var pulseFrequency = (Single)_pulseFrequency.Value / 100f;
        var gradientWidthBaseValue = (Single)_gradientWidth.Value / Byte.MaxValue;

        if (pulseFrequency == 0)
        {
            gradientWidth = gradientWidthBaseValue;
            return;
        }

        gradientWidth = CurrentPulseDirection == PulseDirection.Forward ?
            gradientWidth += gradientWidthBaseValue / 100f * pulseFrequency :
            gradientWidth -= gradientWidthBaseValue / 100f * pulseFrequency;
        if (gradientWidth > gradientWidthBaseValue)
            CurrentPulseDirection = PulseDirection.Backward;
        if (gradientWidth <= 0)
            CurrentPulseDirection = PulseDirection.Forward;
    }

    private enum PulseDirection
    {
        Forward, Backward
    }
}