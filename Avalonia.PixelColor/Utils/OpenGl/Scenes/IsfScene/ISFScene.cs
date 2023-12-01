#nullable enable

using Avalonia.OpenGL;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Avalonia.PixelColor.Utils.OpenGl.Silk;
using CommunityToolkit.Diagnostics;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes
{
    internal sealed class ISFScene : IOpenGlScene
    {
        private GlInterface? _glInterface;
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

        private GlVersion GlVersion { get; }

        private OpenGlSceneParameter[] _parameters;

        public IEnumerable<OpenGlSceneParameter> Parameters => _parameters;

        public OpenGlScenesEnum Scene => OpenGlScenesEnum.ISFScene;

        private Single _timeValue = 0f;

        private String _fragmentShaderSource;

        private String _vertexShaderSource;

        public ISFScene(GlVersion glVersion)
        {
            GlVersion = glVersion;
            _fragmentShaderSource = String.Empty;
            _vertexShaderSource = String.Empty;
            SetUp(
                fragmentShaderSource: ISFFragmentShaderSource,
                vertexShaderSource: ISFVertexShaderSource);
        }

        public void DeInitialize(GlInterface gl)
        {
            _vbo?.Dispose();
            _vao?.Dispose();
            _shader?.Dispose();
        }

        public unsafe void Initialize(GlInterface gl)
        {
            var code = ISFShaderParser.GetShaderCode(ISFFragmentShaderSource);
            _fragmentShaderSource = OpenGlUtils.GetShader(GlVersion, true, code);
            _vertexShaderSource = OpenGlUtils.GetShader(GlVersion, false, ISFVertexShaderSource);
            Guard.IsNotNull(gl);

            _glInterface = gl;
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
                _vertexShaderSource,
                _fragmentShaderSource,
                false);
        }

        private ISFParameters? _isfParameters;

        private IsfSceneParameterOfSingle[] _isfScenePrameters = Array.Empty<IsfSceneParameterOfSingle>();

        [MemberNotNull(nameof(_parameters))]
        public unsafe void SetUp(
            String vertexShaderSource,
            String fragmentShaderSource)
        {
            ISFFragmentShaderSource = fragmentShaderSource;
            ISFVertexShaderSource = vertexShaderSource;
            var isfParameters = ISFShaderParser.GetISFParameters(ISFFragmentShaderSource);
            _parameters = new OpenGlSceneParameter[isfParameters.INPUTS.Length];
            var i = 0;
            var isfSceneParameters = new List<IsfSceneParameterOfSingle>();
            foreach (var input in isfParameters.INPUTS)
            {
                if (input.TYPE == "float")
                {
                    var min = input.MIN;
                    var max = input.MAX;
                    var defaultValue = (Byte)(input.DEFAULT / input.MAX * Byte.MaxValue);
                    var p = new OpenGlSceneParameter(input.NAME, defaultValue);
                    var isfSceneParameter = new IsfSceneParameterOfSingle(
                        sceneParameter: p,
                        min: min,
                        max: max);
                    isfSceneParameters.Add(isfSceneParameter);
                    _parameters[i] = p;
                    ++i;
                }
                else
                {
                    throw new NotSupportedException($"Data type {input.TYPE} not supported");
                }
            }

            _isfParameters = isfParameters;
            _isfScenePrameters = isfSceneParameters.ToArray();
            var gl = _glInterface;
            if (gl is not null)
            {
                var code = ISFShaderParser.GetShaderCode(fragmentShaderSource);
                _fragmentShaderSource = OpenGlUtils.GetShader(GlVersion, true, code);
                _vertexShaderSource = OpenGlUtils.GetShader(GlVersion, false, ISFVertexShaderSource);
                Initialize(gl);
            }
        }

        public void Render(GlInterface gl, int width, int height)
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
                shader.SetUniform("TIME", _timeValue);

                var isfSceneParameters = _isfScenePrameters;
                var parameters = _isfParameters;
                if (isfSceneParameters.Any() && parameters is not null &&
                    isfSceneParameters.Length == parameters.INPUTS.Length)
                {
                    for (var i = 0; i < parameters.INPUTS.Length; i++)
                    {
                        var value = isfSceneParameters[i].CalculateValue();
                        shader.SetUniform(parameters.INPUTS[i].NAME, value);
                    }
                }
            }

            _timeValue += 0.01f;

            silkGl.DrawArrays(
                mode: PrimitiveType.TriangleStrip,
                first: 0,
                count: 4);
        }

        public String ISFFragmentShaderSource { get; set; } =
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

        public String ISFVertexShaderSource =
        @"
        in vec3 position;
		void main() {
			gl_Position = vec4(position, 1.0 );
		}";

    }
}