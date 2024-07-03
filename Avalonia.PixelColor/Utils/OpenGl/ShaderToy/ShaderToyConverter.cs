using System;
using System.Linq;
using System.Text;

namespace Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

public class ShaderToyConverter 
{
    public static String Convert(String source)
    {
        var sb = new StringBuilder();
        
        // фильтр всего, что не ASCII
        foreach (Char c in source.Where(c => c <= 128))
        {
            sb.Append(c);
        }

        var s = sb.ToString();
        
        if (s.Contains(" mainImage")) 
        {
            s = @"uniform vec3 iResolution;
uniform float iTime;
uniform float iTimeDelta;
uniform float iFrameRate;
uniform int iFrame;
uniform float iChannelTime[4];
uniform vec3 iChannelResolution[4];
uniform vec4 iMouse;
uniform vec4 iDate;
uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;
uniform sampler2D iAudioFFT;
uniform sampler2D iAudioSamples;
uniform float iAudioLow;
uniform float iAudioMid;
uniform float iAudioHi;
uniform float iAudioRMS;
uniform float iForce;
uniform float iForce2;
uniform float iForce3;
uniform int iComplexity;
uniform int iNbItems;
uniform int iNbItems2;
uniform int mColorMode;
out vec4 _SYSTEM_outColor;
" + s + @"
void main()
{
  vec4 o = vec4(0.,0.,0.,1.);
  mainImage(o,gl_FragCoord.xy);
  o.w = 1.;
  _SYSTEM_outColor = o;
}";
        }

        if (!s.StartsWith("#version"))
        {
            s = @"
#version 150
precision highp float;
precision highp int;
" + s;
        }

        return s;
    }
}