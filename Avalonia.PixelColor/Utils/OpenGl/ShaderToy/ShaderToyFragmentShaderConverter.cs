using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

public sealed class ShaderToyFragmentShaderConverter : IShaderToyFragmentShaderConverter
{
    public const String ShaderToyBasicShaderParameters = @"#version 400 core

uniform vec3 iResolution;             // viewport resolution (in pixels)
uniform float iTime;                  // shader playback time (in seconds)
uniform float iTimeDelta;             // render time (in seconds)
uniform float iFrameRate;             // shader frame rate
uniform int iFrame;                   // shader playback frame
uniform float iChannelTime[4];        // channel playback time (in seconds)
uniform vec3 iChannelResolution[4];   // channel resolution (in pixels)
uniform vec4 iMouse;                  // image/buffer mouse pixel coords. xy: current (if MLB down). zw: clicked (if any)
uniform vec4 iDate;                   // (year, month, day, time in seconds)
uniform sampler2D iChannel0;          // input channel
uniform samplerCube iChannel1;        // input channel
uniform sampler2D iChannel2;          // input channel
uniform sampler2D iChannel3;          // input channel
uniform sampler2D iAudioFFT;          // audio input
uniform float iForce;
";

    private static readonly Regex _findMainImageRegex = new Regex(
        pattern: @"void\s+mainImage\s*\(.*\)",
        options: RegexOptions.Compiled);

    public String FindMainImage(String shaderToyFragmentShader)
    {
        var result = String.Empty;
        Match match = _findMainImageRegex.Match(shaderToyFragmentShader);
        if (match.Success)
        {
            result = match.Value;
        }

        return result;
    }

    private static readonly Regex _parameterRegex = new Regex(
        pattern: @"\b(in|out)\s+\b(\w+)\b\s+\b(\w+)\b",
        options: RegexOptions.Compiled);

    public IEnumerable<String> GetParameters(String mainFunctionString)
    {
        MatchCollection matches = _parameterRegex.Matches(mainFunctionString);
        foreach (Match match in matches)
        {
            yield return match.Value;
        }
    }

    public String ConvertToOpenGlFragmentShader(String shaderToyFragmentShader)
    {
        StringBuilder result = new(ShaderToyBasicShaderParameters);
        result.AppendLine();
        var mainImage = FindMainImage(shaderToyFragmentShader);
        if (!String.IsNullOrEmpty(mainImage))
        {
            var parameters = GetParameters(mainImage);
            foreach (var parameter in parameters)
            {
                var shaderParameter = $"{parameter};";
                result.AppendLine(shaderParameter);
            }

            var fragmentShader = shaderToyFragmentShader
                .Replace(mainImage, "void main()");
            result.AppendLine(fragmentShader);
        }

        var resultString = result.ToString();
        return resultString;
    }
}