﻿#nullable enable

using Avalonia.OpenGL;
using Common;
using Newtonsoft.Json.Linq;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

internal sealed class IsfScene : IOpenGlScene
{
    private GlInterface? _gl;

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    private static readonly Single[] _vertices =
    [
        //X    Y      Z
        1.0f, 1.0f, 0.0f,
        1.0f, -1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f,
        -1.0f, 1.0f, 0.0f
    ];

    private GlVersion GlVersion { get; }

    private OpenGlSceneParameter[] _parameters;

    public IEnumerable<OpenGlSceneParameter> Parameters => _parameters;

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.IsfScene;

    private Single _timeValue = 0f;

    private String _fragmentShaderSource;

    private String _vertexShaderSource;

    private readonly IIsfShaderParser _isfShaderParser;

    public IsfScene(
        GlVersion glVersion,
        String? fragmentShaderSource = null)
    {
        GlVersion = glVersion;
        _isfShaderParser = new IsfShaderParser();
        _fragmentShaderSource = String.Empty;
        _vertexShaderSource = String.Empty;
        fragmentShaderSource ??= IsfFragmentShaderSource;
        SetUp(
            fragmentShaderSource: fragmentShaderSource,
            vertexShaderSource: IsfVertexShaderSource);
    }

    public void DeInitialize(GlInterface gl)
    {
        gl.DeleteProgram(_program);
        gl.UseProgram(0);
    }

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        gl.ClearColor(r: 0f, g: 0f, b: 0f, a: 1);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

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

        String code = _isfShaderParser
            .GetShaderCode(IsfFragmentShaderSource);
        _fragmentShaderSource = OpenGlUtils.GetShader(
            glVersion: GlVersion,
            fragment: true,
            shader: code);
        _vertexShaderSource = OpenGlUtils.GetShader(
            glVersion: GlVersion,
            fragment: false,
            shader: IsfVertexShaderSource);

        var vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        var error = gl.CompileShaderAndGetError(
            vertexShader,
            _vertexShaderSource);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        var fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        error = gl.CompileShaderAndGetError(
            fragmentShader,
            _fragmentShaderSource);
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

    private IsfParameters? _isfParameters;

    private IIsfParameter[] _isfSceneParameters = Array.Empty<IsfParameterOfSingle>();

    [MemberNotNull(nameof(_parameters))]
    private void SetUp(
        String vertexShaderSource,
        String fragmentShaderSource)
    {
        IsfFragmentShaderSource = fragmentShaderSource;
        IsfVertexShaderSource = vertexShaderSource;
        var isfParameters = _isfShaderParser
            .GetIsfParameters(IsfFragmentShaderSource);
        var parameters = new OpenGlSceneParameter[isfParameters.INPUTS.Length];
        var i = 0;
        var isfSceneParameters = new List<IIsfParameter>();
        foreach (IsfInput input in isfParameters.INPUTS)
        {
            IIsfParameter? isfParameter = null;
            if (input.TYPE == OpenGlConstants.ParameterTypes.Float)
            {
                var min = ConvertToSingle(input.MIN) ?? 0;
                var max = ConvertToSingle(input.MAX) ?? 1;
                var defaultValueSingle = ConvertToSingle(input.DEFAULT) ?? 0;
                var defaultValue = (Byte)(defaultValueSingle / min * Byte.MaxValue);
                var p = new OpenGlSceneParameter(input.NAME, defaultValue);
                parameters[i] = p;
                isfParameter = new IsfParameterOfSingle(
                    sceneParameter: p,
                    min: min,
                    max: max);
            }
            else if (input is { DEFAULT: JArray defaultPointArray })
            {
                Single[] array = defaultPointArray
                    .Select(o => o.Value<Single>())
                    .ToArray();
                isfParameter = input switch
                {
                    { TYPE: OpenGlConstants.ParameterTypes.Point2d }
                        => new IsfParameterOfPoint2d(array),
                    { TYPE: OpenGlConstants.ParameterTypes.Color }
                        => new IsfParameterOfColor(array),
                    _ => isfParameter
                };
            }

            if (isfParameter is not null)
            {
                isfSceneParameters.Add(isfParameter);
                ++i;
            }
        }

        _parameters = parameters
            .Where(a => a is not null)
            .ToArray();
        _isfParameters = isfParameters;
        _isfSceneParameters = isfSceneParameters.ToArray();
        var gl = _gl;
        if (gl is not null)
        {
            String code = _isfShaderParser
                .GetShaderCode(fragmentShaderSource);
            _fragmentShaderSource = OpenGlUtils.GetShader(
                glVersion: GlVersion,
                fragment: true,
                shader: code);
            _vertexShaderSource = OpenGlUtils.GetShader(
                glVersion: GlVersion,
                fragment: false,
                shader: IsfVertexShaderSource);
            Initialize(gl);
        }
    }

    private static Single? ConvertToSingle(Object value)
    {
        Double? result = value switch
        {
            null => null,
            Single f => f,
            Int64 l => l,
            Double d => d,
            _ => throw new NotSupportedException(value.GetType().ToString())
        };
        return (Single?)result;
    }

    public unsafe void Render(GlInterface gl, Int32 width, Int32 height)
    {
        gl.BindVertexArray(_vao);
        gl.UseProgram(_program);
        Int32 renderSize = gl.GetUniformLocationString(_program, "RENDERSIZE");
        if (renderSize != OpenGlConstants.NotFoundIndex)
        {
            gl.Uniform2f(renderSize, width, height);
        }

        Int32 time = gl.GetUniformLocationString(_program, "TIME");
        if (time != OpenGlConstants.NotFoundIndex)
        {
            gl.Uniform1f(time, _timeValue);
        }

        var isfSceneParameters = _isfSceneParameters;
        var parameters = _isfParameters;
        if (isfSceneParameters.Any() && parameters is not null &&
            isfSceneParameters.Length == parameters.INPUTS.Length)
        {
            for (var i = 0; i < parameters.INPUTS.Length; i++)
            {
                var uniform = gl.GetUniformLocationString(_program, parameters.INPUTS[i].NAME);
                if (uniform != OpenGlConstants.NotFoundIndex)
                {
                    IIsfParameter isfParameter = isfSceneParameters[i];
                    if (isfParameter is IsfParameterOfSingle isfParameterOfSingle)
                    {
                        var value = isfParameterOfSingle.CalculateValue();
                        gl.Uniform1f(uniform, value);
                    }
                    else if (isfParameter is IsfParameterOfPoint2d isfParameterOfPoint2d)
                    {
                        gl.Uniform2f(uniform, isfParameterOfPoint2d.X, isfParameterOfPoint2d.Y);
                    }
                    else if (isfParameter is IsfParameterOfColor isfParameterOfColor)
                    {
                        gl.Uniform4fv(
                            location: uniform,
                            x: isfParameterOfColor.Red,
                            y: isfParameterOfColor.Green,
                            z: isfParameterOfColor.Blue,
                            w: isfParameterOfColor.Alpha);
                    }
                }
            }
        }

        _timeValue += 0.01f;

        gl.DrawElements(
            mode: GL_TRIANGLES,
            count: 6,
            type: OpenGlConstants.GL_UNSIGNED_INT,
            indices: IntPtr.Zero);
        Int32 error = gl.GetError();
    }

    public String IsfFragmentShaderSource { get; set; } =
        @"
/*{
	""CREDIT"": ""by mojovideotech"",
	""CATEGORIES"" : [ ""generator""
  ],
  ""DESCRIPTION"" : """",
  ""INPUTS"" : [
	{
     	""NAME"" :		""seed1"",
     	""TYPE"" : 		""float"",
    	""DEFAULT"" :		155,
     	""MIN"" : 		    34,
      	""MAX"" :			233
	},
    {
      	""NAME"" :		""seed2"",
      	""TYPE"" :		""float"",
      	""DEFAULT"" :		649,
      	""MIN"" : 		    89,
      	""MAX"" :			987
	},
	{
		""NAME"" : 		""scale"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	1.0,
		""MIN"" : 		0.25,
		""MAX"" : 		2.0
	},
	{
		""NAME"" : 		""rate"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	1.0,
		""MIN"" : 		0.1,
		""MAX"" : 		3.0
	},
	{
		""NAME"" : 		""zoom"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	0.175,
		""MIN"" : 		-1.0,
		""MAX"" : 		1.0
	},
	{
		""NAME"" : 		""line"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	0.367,
		""MIN"" : 		0.0,
		""MAX"" : 		0.5
	},
	{
		""NAME"" : 		""flash"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	7.5,
		""MIN"" : 		0.5,
		""MAX"" : 		10.0
	},
   	{
		""NAME"" : 		""cycle"",
		""TYPE"" : 		""float"",
		""DEFAULT"" : 	1.0,
		""MIN"" : 		0.05,
		""MAX"" : 		20.0
	}
  ]
}
*/


////////////////////////////////////////////////////////////////////
// InnerDimensionalMatrix  by mojovideotech
//
// based on :
// The Universe Within - by Martijn Steinrucken aka BigWings 2018
// shadertoy.com/\lscczl
// glslsandbox.com\/e#47584.1
//
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0
////////////////////////////////////////////////////////////////////

#ifdef GL_ES
precision highp float;
#endif

#define S(a, b, t) smoothstep(a, b, t)

float N1(float n) {
	return fract(sin(n) * 43758.5453123);
}

float N11(float p) {
	float fl = floor(p);
	float fc = fract(p);
	return mix(N1(fl), N1(fl + 1.0), fc);
}

float N21(vec2 p) { return fract(sin(p.x * floor(seed1) + p.y * floor(seed2)) * floor(seed2+seed1)); }

vec2 N22(vec2 p) { return vec2(N21(p), N21(p + floor(seed2))); }

float L(vec2 p, vec2 a, vec2 b) {
	vec2 pa = p-a, ba = b-a;
	float t = clamp(dot(pa, ba)/dot(ba, ba), 0.0, 1.0);
	float d = length(pa - ba * t);
	float m = S(0.02, 0.0, d);
	d = length(a-b);
	float f = S(1.0, 0.8, d);
	m *= f;
	m += m*S(0.05, 0.06, abs(d - 0.5)) * 2.0;
	return m;
}

vec2 GetPos(vec2 p, vec2 o) {
	p += o;
	vec2 n = N22(p)*TIME*rate;
	p = sin(n) * line;
	return o+p;
}

float G(vec2 uv) {
	vec2 id = floor(uv);
	uv = fract(uv) - 0.5;
	vec2 g = GetPos(id, vec2(0));
	float m = 0.0;
	for(float y=-1.0; y<=1.0; y++) {
		for(float x=-1.0; x<=1.0; x++) {
			vec2 offs = vec2(x, y);
			vec2 p = GetPos(id, offs);
			m+=L(uv, g, p);
			vec2 a = p-uv;
			float f = 0.003/dot(a, a);
			f *= pow( sin(N21(id+offs) * 6.2831 + (flash*TIME)) * 0.4 + 0.6, flash);
			m += f;
		}
	}
	m += L(uv, GetPos(id, vec2(-1, 0)), GetPos(id, vec2(0, -1)));
	m += L(uv, GetPos(id, vec2(0, -1)), GetPos(id, vec2(1, 0)));
	m += L(uv, GetPos(id, vec2(1, 0)), GetPos(id, vec2(0, 1)));
	m += L(uv, GetPos(id, vec2(0, 1)), GetPos(id, vec2(-1, 0)));
	return m;
}

void main()
{
	vec2 uv = (2.25 - scale) * ( gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
	float m = 0.0;
	vec3 col;
	for(float i=0.0; i<1.0; i+=0.2) {
		float z = fract(i+TIME*zoom);
		float s = mix(10.0, 0.5, z);
		float f = S(0.0, 0.4, z) * S(1.0, 0.8, z);
		m += G(uv * s + (N11(i)*100.0) * i) * f;
	}
	col = 0.5 + sin(vec3(1.0, 0.5, 0.75)*TIME*cycle) * 0.5;
	col *= m;
	gl_FragColor = vec4( col, 1.0 );
}
            ";

    public String IsfVertexShaderSource =
        @"
            attribute vec3 position;
		    void main() {
			    gl_Position = vec4(position, 1.0 );
		    }";
}