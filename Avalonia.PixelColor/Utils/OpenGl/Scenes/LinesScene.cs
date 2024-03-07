#nullable enable

using Avalonia.OpenGL;
using Common;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class LinesScene : IOpenGlScene
{
    public LinesScene(GlVersion glVersion)
    {
        GlVersion = glVersion;
        Parameters = Array.Empty<OpenGlSceneParameter>();
    }
    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.Lines;

    private GlInterface? _gl;

    private String FragmentShaderSource => OpenGlUtils.GetShader(GlVersion, true,
        @"
        uniform int PASSINDEX;
        uniform vec2 RENDERSIZE;
        varying vec2 isf_FragNormCoord;
        uniform float TIME;
        uniform float TIMEDELTA;
        uniform vec4 DATE;
        uniform int FRAMEINDEX;
        uniform float spacing;
        uniform float line_width;
        uniform float angle;
        uniform float shift;
        uniform vec4 color1;
        uniform vec4 color2;

        const float pi = 3.14159265359;

        float pattern() {
        float s = sin(angle * pi);
        float c = cos(angle * pi);
        vec2 tex = isf_FragNormCoord * RENDERSIZE;
        float spaced = RENDERSIZE.y * spacing;
        vec2 point = vec2( c * tex.x - s * tex.y, s * tex.x + c * tex.y ) * max(1.0/spaced,0.001);
        float d = point.y;
        float w = line_width;
        if (w > spacing) {
        w = 0.99*spacing; 
        }
        return ( mod(d + shift*spacing + w * 0.5,spacing) );
        }


        void main() {
        // determine if we are on a line
        // math goes something like, figure out distance to the closest line, then draw color2 if we're within range
        // y = m*x + b
        // m = (y1-y0)/(x1-x0) = tan(angle)
        vec4 out_color = color2;
        float w = line_width;
        if (w > spacing) {
        w = 0.99*spacing; 
        }
        float pat = pattern();
        if ((pat > 0.0)&&(pat <= w)) {
        float percent = (1.0-abs(w-2.0*pat)/w);
        percent = clamp(percent,0.0,1.0);
        out_color = mix(color2,color1,percent);
        }
        //gl_FragColor = out_color;
        }");

    private String VertexShaderSource => OpenGlUtils.GetShader(GlVersion, false,
        @"
        attribute vec4 VERTEXDATA; 
        void isf_vertShaderInit(); 
        uniform int PASSINDEX;
        uniform vec2 RENDERSIZE;
        varying vec2 isf_FragNormCoord;
        uniform float TIME;
        uniform float TIMEDELTA;
        uniform vec4 DATE;
        uniform int FRAMEINDEX;
        uniform float spacing;
        uniform float line_width;
        uniform float angle;
        uniform float shift;
        uniform vec4 color1;
        uniform vec4 color2;

        void main(void) { 
        isf_vertShaderInit(); 
        } 

        void isf_vertShaderInit(void) { 
        // gl_Position should be equal to gl_ProjectionMatrix * gl_ModelViewMatrix * gl_Vertex 
        mat4 projectionMatrix = mat4(2./RENDERSIZE.x, 0., 0., -1., 
        0., 2./RENDERSIZE.y, 0., -1., 
        0., 0., -1., 0., 
        0., 0., 0., 1.); 
        gl_Position = VERTEXDATA * projectionMatrix; 
        isf_FragNormCoord = vec2((gl_Position.x+1.0)/2.0, (gl_Position.y+1.0)/2.0); 
        vec2 isf_fragCoord = floor(isf_FragNormCoord * RENDERSIZE); 
        }");

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        gl.ClearColor(r: 0.3922f, g: 0.5843f, b: 0.9294f, a: 1);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);

        var vertices = OpenGlConstants.Vertices;
        fixed (Single* buf = vertices)
        {
            gl.BufferData(
                target: GL_ARRAY_BUFFER,
                size: (IntPtr)(vertices.Length * sizeof(Single)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        var indices = OpenGlConstants.Indices;

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
            VertexShaderSource);
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

        _gl.BindVertexArray(0);
        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void DeInitialize(GlInterface gl)
    {
        gl.DeleteProgram(_program);
        gl.UseProgram(0);
    }

    public void Render(GlInterface gl, int width, int height)
    {
        gl.Viewport(0, 0, width, height);
        
        gl.BindVertexArray(_vao);
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
            type: OpenGlConstants.GL_UNSIGNED_INT,
            indices: IntPtr.Zero);
    }
}