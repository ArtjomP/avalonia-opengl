using Avalonia.OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System;

using static Avalonia.OpenGL.GlConsts;
using CommunityToolkit.Diagnostics;

namespace Avalonia.PixelColor.Utils.OpenGl;

public static class OpenGlUtils
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    public const Int32 RgbaSize = 4;

    public const Int32 XyzSize = 3;

    public static void CheckError(GlInterface gl)
    {
        Int32 err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
        {
            Debug.WriteLine(err);
        }
    }

    public static Vertex[] GetVertices(Single[] points)
    {
        var verticesCount = points.Length / XyzSize;
        var vertices = new Vertex[verticesCount];
        for (var primitive = 0; primitive < verticesCount; primitive++)
        {
            var sourceIndex = primitive * 3;
            vertices[primitive] = new Vertex
            {
                Position = new Vector3(
                    x: points[sourceIndex],
                    y: points[sourceIndex + 1],
                    z: points[sourceIndex + 2])
            };
        }

        return vertices;
    }

    public static String GetShader(GlVersion glVersion, Boolean fragment, String shader)
    {
        var openGlShaderVersion = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? 150
            : 130;
        var shaderVersionToUse =
            glVersion.Type is GlProfileType.OpenGL
            ? openGlShaderVersion
            : 100;
        var data = "#version " + shaderVersionToUse + "\n";
        if (glVersion.Type is GlProfileType.OpenGLES)
        {
            data += "precision mediump float;\n";
        }

        if (shaderVersionToUse >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
            {
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor");
            }
            else
            {
                shader = shader.Replace("varying", "out");
            }
        }

        data += shader;

        return data;
    }

    public static unsafe void MakeScreenshot(
        GlInterface gl,
        Int32 finalWidth,
        Int32 finalHeight)
    {
        var pixels = new Byte[RgbaSize * finalHeight * finalWidth];
        var glExt = new GlExtrasInterface(gl);
        fixed (void* pPixels = pixels)
        {
            glExt.ReadPixels(
                x: 0,
                y: 0,
                width: finalWidth,
                height: finalHeight,
                format: GL_RGBA,
                type: GL_UNSIGNED_BYTE,
                data: pPixels);
            CheckError(gl);
        }
    }

    private static void FillBitmap(
        Int32 width,
        Int32 height,
        Byte[] rgbaData,
        System.Drawing.Bitmap pic)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var arrayOffset = y * width * RgbaSize + x * RgbaSize;
                var c = System.Drawing.Color.FromArgb(
                   rgbaData[arrayOffset + 3],
                   rgbaData[arrayOffset],
                   rgbaData[arrayOffset + 1],
                   rgbaData[arrayOffset + 2]);
                pic.SetPixel(x, y, c);
            }
        }
    }

    public static GradientParameters UpdateGradientWidth(
       GradientParameters gradientParameters,
       OpenGlSceneParameter pulse,
       OpenGlSceneParameter leftWidth,
       OpenGlSceneParameter rightWidth)
    {
        Guard.IsNotNull(gradientParameters);
        Guard.IsNotNull(pulse);
        Guard.IsNotNull(leftWidth);
        Guard.IsNotNull(rightWidth);
        var pulseFrequency = (Single)pulse.Value;
        var leftGradientWidthBaseValue = (Single)leftWidth.Value / 100f;
        var rightGradientWidthBaseValue = (Single)rightWidth.Value / 100f;
        var resultLeftWidth = gradientParameters.leftGradientWidth;
        var resultRightWidth = gradientParameters.rightGradientWidth;
        var direction = gradientParameters.direction;
        if (pulseFrequency == 0)
        {
            resultLeftWidth = leftGradientWidthBaseValue;
            resultRightWidth = rightGradientWidthBaseValue;
        }
        else
        {
            if (leftGradientWidthBaseValue > 0)
            {
                resultLeftWidth = CalculateWidth(
                    pulseFrequency: pulseFrequency,
                    baseWidth: leftGradientWidthBaseValue,
                    targetWidth: resultLeftWidth,
                    direction: direction);
            }
            else
            {
                resultLeftWidth = 0;
            }

            if (rightGradientWidthBaseValue > 0)
            {
                resultRightWidth = CalculateWidth(
                    pulseFrequency: pulseFrequency,
                    baseWidth: rightGradientWidthBaseValue,
                    targetWidth: resultRightWidth,
                    direction: direction);
            }
            else
            {
                resultRightWidth = 0;
            }

            var baseValue = Math.Max(leftGradientWidthBaseValue, rightGradientWidthBaseValue);
            var value = leftGradientWidthBaseValue > rightGradientWidthBaseValue
                ? resultLeftWidth
                : resultRightWidth;
            if (value >= baseValue)
            {
                direction = Direction.Backward;
            }

            if (value <= 0)
            {
                direction = Direction.Forward;
            }
        }

        var result = new GradientParameters(
            direction: direction,
            leftGradientWidth: resultLeftWidth,
            rightGradientWidth: resultRightWidth);
        return result;
    }

    private static Single CalculateWidth(
        Single pulseFrequency,
        Single baseWidth,
        Single targetWidth,
        Direction direction)
    {
        var offset = baseWidth / 100f * pulseFrequency;
        var directionCoeff = direction is Direction.Forward
            ? +1
            : -1;
        targetWidth = targetWidth + offset * directionCoeff;
        targetWidth = Math.Clamp(
            value: targetWidth,
            min: 0,
            max: baseWidth);
        return targetWidth;
    }
}