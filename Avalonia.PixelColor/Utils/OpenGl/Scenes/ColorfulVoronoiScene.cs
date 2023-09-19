#nullable enable

using Avalonia.OpenGL;
using Common;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

//https://glslsandbox.com/e#103855.0
internal sealed class ColorfulVoronoi : IOpenGlScene
{
    private readonly OpenGlSceneParameter _speed;
    private readonly OpenGlSceneParameter _lineWidth;
    private readonly OpenGlSceneParameter _innerGradientWidthParameter;
    private readonly OpenGlSceneParameter _outerGradientWidthParameter;
    private readonly OpenGlSceneParameter _timePulseRange;
    private readonly OpenGlSceneParameter _gradientPulseFrequency;

    public ColorfulVoronoi(GlVersion glVersion)
    {
        _speed = new OpenGlSceneParameter("Speed", 55, 0, 100);
        _lineWidth = new OpenGlSceneParameter("Line width", 20, 0, 40);
        _innerGradientWidthParameter = new OpenGlSceneParameter("Inner gradient width", 20, 0, 40);
        _outerGradientWidthParameter = new OpenGlSceneParameter("Outer gradient width", 20, 0, 40);
        _timePulseRange = new OpenGlSceneParameter("Time pulse range", 5, 0, 10);
        _gradientPulseFrequency = new OpenGlSceneParameter("Gradient pulse frequency", 5, 0, 10);
        GlVersion = glVersion;

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

    private Single _timeValue = 0f;

    private GradientParameters _gradientParameters = new(
        direction: Direction.Backward,
        leftGradientWidth: 0,
        rightGradientWidth: 0);

    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ColorfulVoronoi;

    private GlInterface? _gl;

    private GlExtrasInterface? _glExtras;

    private String FragmentShaderSource => OpenGlUtils.GetShader(GlVersion, true,
        @"     
        precision highp float;

        uniform float time;
        uniform float resolution_x;
        uniform float resolution_y;
        uniform float line_width;
        uniform float inner_gradient_width;
        uniform float outer_gradient_width;

      //  out vec4 gl_FragColor;

        //https://iquilezles.org/articles/palettes/
        vec3 palette( float t ) {
            vec3 a = vec3(0.5, 0.5, 0.5);
            vec3 b = vec3(0.5, 0.5, 0.5);
            vec3 c = vec3(1.0, 1.0, 1.0);
            vec3 d = vec3(0.263,0.416,0.557);

            return a + b * cos(6.28318 * (c * t + d));
        }

        void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
            vec2 resolution = vec2(resolution_x, resolution_y);
            
            vec2 uv = (fragCoord * 2.0 - resolution.xy) / min(resolution.x, resolution.y);
            vec2 uv0 = uv;
            vec3 finalColor = vec3(0.0);

            for (float i = 0.0; i < 4.0; i++) {
                uv = fract(uv * 1.5) - 0.5;

                float d = length(uv) * exp(-length(uv0));

                vec3 col = palette(length(uv0) + i*.4 + time*.4);

                d = tan(d * 8. + time)/8.;
                float s = d; //tan(d * 8. + time)/8.;
                d = abs(d);                
                d = pow(0.01 / d, 1.2);
                float p = clamp(d * (line_width + outer_gradient_width + inner_gradient_width), 0.0, 1.0);
                float lw = 1. - line_width;
                if(p > lw)
                    finalColor += col;
                if(s >= 0 && p >= lw - outer_gradient_width && p <= lw) 
                    finalColor += col * p * 0.9;
                if(s < 0 && p >= lw - inner_gradient_width && p <= lw) 
                    finalColor += col * p * 0.9;
            }
           
            fragColor = vec4(finalColor, 1.0);
        }

        void main() {
	        vec4 fragment_color;
	        mainImage(fragment_color, gl_FragCoord.xy);
	        gl_FragColor = fragment_color;
        }
        ");

    private String VertexShaderSource => OpenGlUtils.GetShader(GlVersion, false,
        @"attribute vec3 position;
		void main() {
			gl_Position = vec4( position, 1.0 );
		}");

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    private static readonly Single[] _vertices =
    {
            //X    Y      Z 
        1.0f,  1.0f, 0.0f,
        1.0f, -1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f,
        -1.0f,  1.0f, 0.0f
    };

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        gl.ClearColor(r: 0f, g: 0f, b: 0f, a: 1);

        _glExtras ??= new GlExtrasInterface(gl);
        _vao = _glExtras.GenVertexArray();
        _glExtras.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
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

        _glExtras.BindVertexArray(0);
        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void DeInitialize(GlInterface gl)
    {
        gl.DeleteProgram(_program);
        gl.UseProgram(0);
    }

    private Random _random = new Random();

    public void Render(GlInterface gl, Int32 width, Int32 height)
    {
        gl.Viewport(0, 0, width, height);
        var glExtras = _glExtras;
        if (glExtras is not null)
        {
            glExtras.BindVertexArray(_vao);
            gl.UseProgram(_program);

            var w = gl.GetUniformLocationString(_program, "resolution_x");
            gl.Uniform1f(w, width);
            var h = gl.GetUniformLocationString(_program, "resolution_y");
            gl.Uniform1f(h, height);

            var lineWidth = gl.GetUniformLocationString(_program, "line_width");
            gl.Uniform1f(lineWidth, (Single)_lineWidth.Value / 100f);

            UpdateGradientWidth();
            var innerGradientWidth = gl.GetUniformLocationString(_program, "inner_gradient_width");
            var innerGradientWidthValue = _gradientParameters.leftGradientWidth;
            gl.Uniform1f(innerGradientWidth, innerGradientWidthValue);
            var outerGradientWidth = gl.GetUniformLocationString(_program, "outer_gradient_width");
            var outerGradientWidthValue = _gradientParameters.rightGradientWidth;
            gl.Uniform1f(outerGradientWidth, outerGradientWidthValue);

            var time = gl.GetUniformLocationString(_program, "time");
            gl.Uniform1f(time, _timeValue);

            var speed = _speed.Value / 1000f;
            var offset = _timePulseRange.Value == 0
                ? 0
                : _random.Next(-_timePulseRange.Value, _timePulseRange.Value + 1) / 100f;
            _timeValue += speed + offset;

            gl.DrawElements(
                mode: GL_TRIANGLES,
                count: 6,
                type: GL_UNSIGNED_INT,
                indices: IntPtr.Zero);
        }
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