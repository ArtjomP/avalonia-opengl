﻿#nullable enable

using Avalonia.OpenGL;
using Common;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

//https://glslsandbox.com/e#103855.0
internal sealed class ColorfulVoronoi : IOpenGlScene
{
    public ColorfulVoronoi(GlVersion glVersion)
    {
        GlVersion = glVersion;
        Parameters = Array.Empty<OpenGlSceneParameter>();
    }

    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ColorfulVoronoi;

    private GlInterface? _gl;

    private GlExtrasInterface? _glExtras;

    private String FragmentShaderSource => OpenGlUtils.GetShader(GlVersion, true,
@"
precision highp float;

uniform vec2 resolution;
uniform sampler2D texture;

void main() {

	vec2 uv = gl_FragCoord.xy / resolution.xy;
	gl_FragColor = texture2D( texture, uv );

}");

    private String VertexShaderSource => OpenGlUtils.GetShader(GlVersion, false,
        @"
attribute vec3 position;

void main() {

	gl_Position = vec4( position, 1.0 );

}");

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        gl.ClearColor(r: 0.3922f, g: 0.5843f, b: 0.9294f, a: 1);

        _glExtras ??= new GlExtrasInterface(gl);
        _vao = _glExtras.GenVertexArray();
        _glExtras.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);

        var vertices = Constants.Vertices;
        fixed (Single* buf = vertices)
        {
            gl.BufferData(
                target: GL_ARRAY_BUFFER,
                size: (IntPtr)(vertices.Length * sizeof(Single)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        var indices = Constants.Indices;
        _ebo = gl.GenBuffer();
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);

        fixed (UInt32* buf = indices)
        {
            gl.BufferData(
                target: GL_ELEMENT_ARRAY_BUFFER,
                size: (IntPtr)(indices.Length * sizeof(uint)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        var vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        var error = gl.CompileShaderAndGetError(
            vertexShader,
            ConstantStrings.VertexShader);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        var fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        error = gl.CompileShaderAndGetError(
            fragmentShader,
            FragmentShaderSource);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        _program = gl.CreateProgram();
        gl.AttachShader(_program, vertexShader);
        gl.AttachShader(_program, fragmentShader);
        error = gl.LinkProgramAndGetError(_program);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        const Int32 positionLoc = 0;
        gl.EnableVertexAttribArray(positionLoc);
        gl.VertexAttribPointer(
            index: positionLoc,
            size: 3,
            type: GL_FLOAT,
            normalized: 1,
            stride: 3 * sizeof(Single),
            pointer: IntPtr.Zero);

        _glExtras.BindVertexArray(0);
        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void DeInitialize(GlInterface gl)
    {
        gl.DeleteProgram(_program);
        gl.UseProgram(0);
    }

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        gl.Viewport(0, 0, width, height);
        var glExtras = _glExtras;
        if (glExtras is not null)
        {
            glExtras.BindVertexArray(_vao);
            gl.UseProgram(_program);
            var spacing = gl.GetUniformLocationString(_program, "spacing");
            var lineWidth = gl.GetUniformLocationString(_program, "line_width");
            var angle = gl.GetUniformLocationString(_program, "angle");
            var shift = gl.GetUniformLocationString(_program, "shift");
            var color1 = gl.GetUniformLocationString(_program, "color1");
            var color2 = gl.GetUniformLocationString(_program, "color2");
            gl.Uniform1f(spacing, 0.3f);
            gl.Uniform1f(lineWidth, 0.01f);
            gl.Uniform1f(angle, 0.24f);
            gl.Uniform1f(shift, 0.4f);
            gl.DrawElements(
                mode: GL_TRIANGLES,
                count: 6,
                type: GL_UNSIGNED_INT,
                indices: IntPtr.Zero);
        }
    }
}
