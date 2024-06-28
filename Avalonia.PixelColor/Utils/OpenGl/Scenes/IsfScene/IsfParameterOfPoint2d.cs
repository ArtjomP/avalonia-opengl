using Common;
using CommunityToolkit.Diagnostics;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfParameterOfPoint2d : IIsfParameter
{
    public String Type => OpenGlConstants.ParameterTypes.Point2d;

    public IsfParameterOfPoint2d(
        Single[] data)
    {
        Guard.IsNotNull(data);
        Guard.IsEqualTo(data.Length, 2);

        X = data[0];
        Y = data[1];
    }

    public Single X { get; }

    public Single Y { get; }
}