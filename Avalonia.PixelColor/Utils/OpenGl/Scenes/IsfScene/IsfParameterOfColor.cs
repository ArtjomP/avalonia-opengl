using Common;
using CommunityToolkit.Diagnostics;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfParameterOfColor : IIsfParameter
{
    public String Type => OpenGlConstants.ParameterTypes.Color;

    public IsfParameterOfColor(
        Single[] data)
    {
        Guard.IsNotNull(data);
        Guard.IsEqualTo(data.Length, 4);

        Red = data[0];
        Green = data[1];
        Blue = data[2];
        Alpha = data[3];
    }

    public Single Red { get; }

    public Single Green { get; }

    public Single Blue { get; }

    public Single Alpha { get; }
}