#nullable enable

using Avalonia.OpenGL;
using System;
using System.Collections.Generic;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

internal sealed class Lines3Scene : IOpenGlScene
{
    public Lines3Scene(GlVersion glVersion)
    {
        GlVersion = glVersion;
        Parameters = new OpenGlSceneParameter[]
        {
            new OpenGlSceneParameter("Test3", 230),
        };
    }
    private GlVersion GlVersion { get; }

    public IEnumerable<OpenGlSceneParameter> Parameters { get; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.Lines3;

    private GlInterface? _gl;

    private GlExtrasInterface? _glExtras;

    private Single[] _vertices = new[]
    {
        0.0f, 0.0f,
        0.5f, 0.0f,
        0.5f, 0.5f,

        0.0f, 0.0f,
        0.0f, 0.5f,
        -0.5f, 0.5f,

        0.0f, 0.0f,
        -0.5f, 0.0f,
        -0.5f, -0.5f,

        0.0f, 0.0f,
        0.0f, -0.5f,
        0.5f, -0.5f,
    };

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
gl_FragColor = out_color;
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

    private Int32 _vbo;

    private Int32 _program;

    public unsafe void Initialize(GlInterface gl)
    {
        _gl = gl;
        _glExtras ??= new GlExtrasInterface(gl);
        if (gl is not null)
        {

        }        
    }

    public void DeInitialize(GlInterface gl)
    {
        if (gl is not null)
        {
            gl.DeleteProgram(_program);
            gl.UseProgram(0);
        }       
    }

    public void Render(GlInterface gl, int width, int height)
    {
        gl.Viewport(0, 0, width, height);
        var glExtras = _glExtras;
        if (glExtras is not null)
        {
        }
    }
}
