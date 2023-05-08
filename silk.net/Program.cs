using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace silk.net;

class Program
{
    private const Int32 Width = 800;
    private const Int32 Height = 600;
    private const Int32 RgbaSize = 4;
    private static Int32 _counter = 0;

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        var options = WindowOptions.Default with
        {
            Size = new Silk.NET.Maths.Vector2D<Int32>(Width, Height),
            Title = "My first Silk.NET application!",
        };
        _window = Silk.NET.Windowing.Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }

    private static IWindow _window;

    private static GL _gl;

    private static UInt32 _vao;
    private static UInt32 _vbo;
    private static UInt32 _ebo;

    private static UInt32 _vaoForGrab;
    private static UInt32 _vboForGrab;
    private static UInt32 _eboForGrab;

    private static UInt32 _program;

    private static UInt32 _programGrab;

    private static UInt32 _pixelsTexture;

    private static UInt32 _fboForPixels;

    private const String _vertexCode = @"
#version 330 core

layout (location = 0) in vec3 aPosition;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
}";

    private const String _fragmentCode = @"
#version 330 core

out vec4 out_color;

void main()
{
    out_color = vec4(1.0, 0.5, 0.2, 1.0);
}";

    private static readonly String _vertexShaderGrab = @"
#version 330 core
precision mediump float;
uniform mat4 modelViewProjectionMatrix;
uniform ivec2 outTexResolution;

layout (location = 0) in vec3 VertexPosition;

out vec2 ledPos;

void main()
{
    ledPos = VertexPosition.xy;
    gl_Position = modelViewProjectionMatrix
                  * vec4(gl_VertexID % outTexResolution.x, 1 + (gl_VertexID / outTexResolution.x),
                         0.0, 1.0);
}";

    private static readonly String _fragmentShaderGrab = @"
#version 330 core
uniform sampler2DRect texIn;
vec2 ledPos;
out vec4 vFragColor;

vec4 colorConvert(vec4 color) { return vec4(color.r, color.g, color.b, 1.0); }

void main()
{
    vFragColor = colorConvert(texture(texIn, ledPos));
}";

    private unsafe static void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(System.Drawing.Color.CornflowerBlue);

        Console.WriteLine("Load!");
        var input = _window.CreateInput();
        for (var i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        InitRenderingProgram();

        InitGrabProgram();
        //InitRenderToTexture();       
    }

    private static void InitRenderToTexture()
    {
        #region Render to texture
        _gl.GenTextures(1, out _pixelsTexture);
        _gl.BindTexture(GLEnum.Texture2D, _pixelsTexture);
        //var expectedLength = Width * Height * RgbaSize;
        //if (_pixelsTexture0.Length != expectedLength)
        //{
        //    _pixelsTexture0 = new Byte[expectedLength];
        //}

        //fixed (void* pPixels = _pixelsTexture0)
        //{
        //    _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, Width, Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pPixels);
        //}

        //_gl.GenFramebuffers(1, out _fboForPixels);
        //_gl.BindFramebuffer(GLEnum.Framebuffer, _fboForPixels);
        //_gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, GLEnum.ColorAttachment0, TextureTarget.Texture2D, _pixelsTexture, 0);
        #endregion
    }

    private static unsafe void InitGrabProgram()
    {
        _vaoForGrab = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // The quad vertices data.
        Single[] vertices =
        {
            0.85f,  0.85f, 0.0f,
            0.85f, -0.85f, 0.0f,
            -0.85f, -0.85f, 0.0f,
            -0.85f,  0.85f, 0.0f
        };

        // Create the VBO.
        _vboForGrab = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboForGrab);

        // Upload the vertices data to the VBO.
        fixed (Single* buf = vertices)
        {
            _gl.BufferData(
                target: BufferTargetARB.ArrayBuffer, 
                size: (UIntPtr)(vertices.Length * sizeof(Single)), 
                data: buf,
                usage: BufferUsageARB.StaticDraw);
        }

        // The quad indices data.
        UInt32[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        // Create the EBO.
        _eboForGrab = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _eboForGrab);

        // Upload the indices data to the EBO.
        fixed (UInt32* buf = indices)
        {
            _gl.BufferData(
                target: BufferTargetARB.ElementArrayBuffer, 
                size: (UIntPtr)(indices.Length * sizeof(UInt32)),
                data: buf, 
                usage: BufferUsageARB.StaticDraw);
        }

        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, _vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var vStatus);
        if (vStatus != (Int32)GLEnum.True)
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, _fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out var fStatus);
        if (fStatus != (Int32)GLEnum.True)
        {
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
        }

        _programGrab = _gl.CreateProgram();

        _gl.AttachShader(_programGrab, vertexShader);
        _gl.AttachShader(_programGrab, fragmentShader);

        _gl.LinkProgram(_programGrab);

        _gl.GetProgram(_programGrab, ProgramPropertyARB.LinkStatus, out var lStatus);
        if (lStatus != (Int32)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_programGrab));

        _gl.DetachShader(_programGrab, vertexShader);
        _gl.DetachShader(_programGrab, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const UInt32 positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(
            index: positionLoc, 
            size: 3,
            type: VertexAttribPointerType.Float,
            normalized: false,
            stride: 3 * sizeof(Single),
            pointer: (void*)0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    private static unsafe void InitRenderingProgram()
    {
        // Create the VAO.
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // The quad vertices data.
        Single[] vertices =
        {
            0.85f,  0.85f, 0.0f,
            0.85f, -0.85f, 0.0f,
            -0.85f, -0.85f, 0.0f,
            -0.85f,  0.85f, 0.0f
        };

        // Create the VBO.
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Upload the vertices data to the VBO.
        fixed (Single* buf = vertices)
        {
            _gl.BufferData(
                target: BufferTargetARB.ArrayBuffer,
                size: (UIntPtr)(vertices.Length * sizeof(Single)), 
                data: buf,
                usage: BufferUsageARB.StaticDraw);
        }

        // The quad indices data.
        UInt32[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        // Create the EBO.
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // Upload the indices data to the EBO.
        fixed (UInt32* buf = indices)
        {
            _gl.BufferData(
                target: BufferTargetARB.ElementArrayBuffer, 
                size: (UIntPtr)(indices.Length * sizeof(UInt32)),
                data: buf,
                usage: BufferUsageARB.StaticDraw);
        }

        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, _vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var vStatus);
        if (vStatus != (Int32)GLEnum.True)
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, _fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out var fStatus);
        if (fStatus != (Int32)GLEnum.True)
        {
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
        }

        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (Int32)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const UInt32 positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(
            index: positionLoc, 
            size: 3, 
            type: VertexAttribPointerType.Float, 
            normalized: true,
            stride: 3 * sizeof(float),
            pointer: (void*)0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }
     
    private static Byte[] _pixelsTexture0 = Array.Empty<Byte>();

    // These two methods are unused for this tutorial, aside from the logging we added earlier.
    private static void OnUpdate(double dt)
    {
        Console.WriteLine("Update!");
    }

    private static unsafe void OnRender(Double dt)
    {
        Console.WriteLine("Render!");
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

    //    _gl.BindVertexArray(_vaoForGrab);
    //    _gl.UseProgram(_programGrab);
    //    _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

        ++_counter;
        ScreenshotWithReadPixels();
     //   ScreenshotWithTexture();
    }

    private static unsafe void ScreenshotWithTexture()
    {
        var width = Width;
        var height = Height;
        var pixels = new Byte[width * height * RgbaSize];
        fixed (void* pPixels = pixels)
        {
            _gl.GetTexImage(GLEnum.Texture2D, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pPixels);
        }

        if (_counter % 1000 == 100)
        {
            SaveScreenshot("texture", pixels, width, height);
        }
    }

    private static unsafe void ScreenshotWithReadPixels()
    {
        _gl.PointSize(1.0f);
        _gl.DisableVertexAttribArray(0);
        _gl.Flush();
        _gl.Finish();
        _gl.PixelStore(GLEnum.UnpackAlignment, 1);
        var width = Convert.ToUInt32(Width);
        var height = Convert.ToUInt32(Height);
        var pixels = new Byte[RgbaSize * width * height];
        fixed (void* pPixels = pixels)
        {
            //_gl.PixelStore(GLEnum.PackImageHeight, height / 4);
            //_gl.PixelStore(GLEnum.LineWidth, width / 4);
            //_gl.PixelStore(GLEnum.PackAlignment, 1);
            _gl.ReadBuffer(GLEnum.Front);
            _gl.ReadPixels(0, 0, width, height, GLEnum.Rgba, GLEnum.UnsignedByte, pPixels);
        }

        if (_counter % 1000 == 100)
        {
            SaveScreenshot("readPixels", pixels, (Int32)width, (Int32)height);
        }
    }

    private static void SaveScreenshot(String prefix, Byte[] rgbaData, Int32 width, Int32 height)
    {
        //height = 1;
        using var image = new Image<Rgba32>(width, height);
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = (y * width) + x * RgbaSize;
                    Rgba32 pixel = image[x, y];
                    pixel.R = rgbaData[index];
                    pixel.G = rgbaData[index + 1];
                    pixel.B = rgbaData[index + 2];
                    pixel.A = rgbaData[index + 3];
                    image[x, y] = pixel;
                }
            }
        }

        var filename = "c:\\temp\\";
        filename = Path.Combine(filename, $"silk-{prefix}-{Guid.NewGuid()}.bmp");
        image.SaveAsBmp(filename);
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}
