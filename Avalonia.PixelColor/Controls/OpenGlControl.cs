using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Common;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Controls;

public sealed class OpenGlControl : OpenGlControlBase
{
    private GlInterface? _gl;

    private GlExtrasInterface? _glExtras;

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    static OpenGlControl()
    {

    }

    public OpenGlControl()
    {
    }

    public Double ScaleFactor { get; set; } = 1;

    public List<TrackPoint> TrackPoints
    {
        get;
    } = new List<TrackPoint>();

    protected override unsafe void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);
        _gl = gl;
        gl.ClearColor(r: 0.3922f, g: 0.5843f, b: 0.9294f, a: 1);

        _glExtras = new GlExtrasInterface(gl);
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
                size: (IntPtr)(indices.Length * sizeof(UInt32)),
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
            ConstantStrings.FragmentShader);
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

    private unsafe void GetTrackPointsColors(Int32 width, Int32 height)
    {
        var gl = _gl;
        var glExtras = _glExtras;
        if (gl is not null && glExtras is not null)
        {
            foreach (var trackPoint in TrackPoints)
            {
                var x = (Int32)Math.Round(trackPoint.ReleativeX * width);
                var y = (Int32)Math.Round(trackPoint.ReleativeY * height);
                var pixels = new Byte[Constants.RgbaSize];
                fixed (void* pPixels = pixels)
                {
                    glExtras.ReadPixels(
                        x: x,
                        y: y,
                        width: 1,
                        height: 1,
                        format: GL_RGBA,
                        type: GL_UNSIGNED_BYTE,
                        data: pPixels);
                }

                var r = pixels[0];
                var g = pixels[1];
                var b = pixels[2];
                var a = pixels[3];
                var color = System.Drawing.Color.FromArgb(a, r, g, b);
                trackPoint.Color = color;
            }
        }
    }

    private unsafe void DoScreenShot(
        String fullname,
        Int32 width,
        Int32 height,
        Double scaleFactor)
    {
        var gl = _gl;
        var glExtras = _glExtras;
        if (gl is not null && glExtras is not null)
        {
            var pixelSize = Constants.RgbaSize;
            var newPixelSize = (Int32)(pixelSize * scaleFactor);
            var pixelsCount = (Int32)newPixelSize * width * height;
            var pixels = new Byte[pixelsCount];
            gl.Finish();
            glExtras.ReadBuffer(GL_COLOR_ATTACHMENT0);
            fixed (void* pPixels = pixels)
            {
                glExtras.ReadPixels(
                    x: 0,
                    y: 0,
                    width: width,
                    height: height,
                    format: GL_RGBA,
                    type: GL_UNSIGNED_BYTE,
                    data: pPixels);
            }

            Utils.SaveScreenshot(pixels, width, height, pixelSize, fullname);
        }
    }

    private String _screeshotFullname = String.Empty;

    public void MakeScreenShot(String fullname)
    {
        _screeshotFullname = fullname;
    }

    protected override void OnOpenGlDeinit(GlInterface gl, Int32 fb)
    {
        base.OnOpenGlDeinit(gl, fb);
    }

    protected override void OnOpenGlRender(GlInterface gl, Int32 fb)
    {
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        var width = (Int32)Bounds.Width;
        var height = (Int32)Bounds.Height;

        var scaleFactor = ScaleFactor;
        var finalWidth = (Int32)(width * scaleFactor);
        var finalHeight = (Int32)(height * scaleFactor);
        gl.Viewport(0, 0, finalWidth, finalHeight);

        var glExtras = _glExtras;
        if (glExtras is not null)
        {
            glExtras.BindVertexArray(_vao);
            gl.UseProgram(_program);
            gl.DrawElements(
                mode: GL_TRIANGLES,
                count: 6,
                type: GL_UNSIGNED_INT,
                indices: IntPtr.Zero);

            GetTrackPointsColors(finalWidth, finalHeight);
            var screeshotFullname = _screeshotFullname;
            _screeshotFullname = String.Empty;
            if (!String.IsNullOrEmpty(screeshotFullname))
            {
                DoScreenShot(screeshotFullname, finalWidth, finalHeight, scaleFactor);
            }
        }
    }

    private class GlExtrasInterface : GlInterfaceBase<GlInterface.GlContextInfo>
    {
        public GlExtrasInterface(GlInterface gl)
            : base(gl.GetProcAddress, gl.ContextInfo)
        {
        }

        public unsafe delegate void GlGetTexImage(Int32 target, Int32 level, Int32 format, Int32 type, void* pixels);
        [GlMinVersionEntryPoint("glGetTexImage", 3, 0)]
        public GlGetTexImage GetTexImage { get; }

        public unsafe delegate void GlPixelStore(Int32 parameterName, Int32 parameterValue);
        [GlMinVersionEntryPoint("glPixelStorei", 3, 0)]
        public GlPixelStore PixelStore { get; }

        public unsafe delegate void GlReadBuffer(Int32 mode);
        [GlMinVersionEntryPoint("glReadBuffer", 3, 0)]
        public GlReadBuffer ReadBuffer { get; }

        public unsafe delegate void GlReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, Int32 format, Int32 type, void* data);
        [GlMinVersionEntryPoint("glReadPixels", 3, 0)]
        public GlReadPixels ReadPixels { get; }

        public delegate void GlDeleteVertexArrays(Int32 count, Int32[] buffers);
        [GlMinVersionEntryPoint("glDeleteVertexArrays", 3, 0)]
        [GlExtensionEntryPoint("glDeleteVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlDeleteVertexArrays DeleteVertexArrays { get; }

        public delegate void GlBindVertexArray(Int32 array);
        [GlMinVersionEntryPoint("glBindVertexArray", 3, 0)]
        [GlExtensionEntryPoint("glBindVertexArrayOES", "GL_OES_vertex_array_object")]
        public GlBindVertexArray BindVertexArray { get; }
        public delegate void GlGenVertexArrays(Int32 n, Int32[] rv);

        [GlMinVersionEntryPoint("glGenVertexArrays", 3, 0)]
        [GlExtensionEntryPoint("glGenVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlGenVertexArrays GenVertexArrays { get; }

        public Int32 GenVertexArray()
        {
            var rv = new Int32[1];
            GenVertexArrays(1, rv);
            return rv[0];
        }
    }
}