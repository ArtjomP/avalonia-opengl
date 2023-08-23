using System;
using System.Numerics;
using Avalonia.OpenGL;
using Common;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl;

public class Line : IDisposable
{
    private Int32 _shaderProgram;

    private Int32 _vbo;

    private Int32 _vao;

    private Single[] _vertices;

    private Vector3 _startPoint;

    private Vector3 _endPoint;

    private Matrix4x4 _mvp;

    private float[] _lineColor;

    private GlInterface _gl;

    private GlExtrasInterface _glExtras;
    private bool disposedValue;

    public unsafe Line(
        GlVersion glVersion,
        GlInterface gl,
        Vector3 startPoint,
        Vector3 endPoint)
    {
        _gl = gl;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _lineColor = new float[] { 0, 1, 0.5f };
        _mvp = Matrix4x4.Identity;
        var vertexShaderSource =
            @"#version 330 core
            layout (location = 0) in vec3 aPos;
            uniform mat4 MVP;
            void main()
            {
               gl_Position = MVP * vec4(aPos, 1.0);
            }";

        var fragmentShaderSource =
            @"#version 330 core
            out vec4 FragColor;
            uniform vec3 color;
            void main()
            {
                FragColor = vec4(color, 1.0f);
            }";
        var vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        var error = gl.CompileShaderAndGetError(
           vertexShader,
           vertexShaderSource);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        var fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        error = gl.CompileShaderAndGetError(
            fragmentShader,
            fragmentShaderSource);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }


        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, vertexShader);
        gl.AttachShader(_shaderProgram, fragmentShader);
        gl.LinkProgram(_shaderProgram);

        gl.DeleteShader(fragmentShader);
        gl.DeleteShader(vertexShader);

        _vertices = new[]
        {
            startPoint.X, startPoint.Y, startPoint.Z,
            endPoint.X, endPoint.Y, endPoint.Z
        };

        var glExtras = new GlExtrasInterface(gl);
        _glExtras = glExtras;
        _vao = glExtras.GenVertexArray();
        _vbo = gl.GenBuffer();
        glExtras.BindVertexArray(_vao);

        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
        var vertices = _vertices;
        fixed (Single* buf = vertices)
        {
            gl.BufferData(
                target: GL_ARRAY_BUFFER,
                size: (IntPtr)(vertices.Length * sizeof(Single)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        gl.VertexAttribPointer(
            index: 0,
            size: 3,
            type: GL_FLOAT,
            normalized: 0,
            stride: 3 * sizeof(Single),
            pointer: IntPtr.Zero);
        gl.EnableVertexAttribArray(0);

        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        glExtras.BindVertexArray(0);
    }


    public Int32 SetMvp(Matrix4x4 mvp)
    {
        _mvp = mvp;
        return 1;
    }

    public Int32 SetColor(Vector3 color)
    {
        _lineColor = new float[] { color.X, color.Y, color.Z };
        return 1;
    }

    public Int32 SetColor(float red, float green, float blue, float alpha)
    {
        _lineColor = new float[] { red, green, blue, alpha };
        return 1;
    }

    public unsafe void Draw()
    {
        _gl.UseProgram(_shaderProgram);
        var location = _gl.GetUniformLocationString(_shaderProgram, "MVP");
        var mvp = _mvp;
        _gl.UniformMatrix4fv(location, 1, false, &mvp);
        location = _gl.GetUniformLocationString(_shaderProgram, "color");
        Single[] lineColor = _lineColor;
        fixed (Single* lineColorPtr = lineColor)
        {
            _gl.Uniform3fv(location, 1, lineColorPtr);
        }
        _gl.LineWidth(3);
        _glExtras.BindVertexArray(_vao);
        _gl.DrawArrays(GL_LINES, 0, (IntPtr)2);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {                
            }

            _glExtras.DeleteVertexArrays(1, new Int32[] { _vao });
            _gl.DeleteBuffers(1, new Int32[] { _vbo });
            _gl.DeleteProgram(_shaderProgram);
            disposedValue = true;
        }
    }

    ~Line()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
