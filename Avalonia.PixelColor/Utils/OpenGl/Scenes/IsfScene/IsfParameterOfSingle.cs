using Common;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfParameterOfSingle(
    OpenGlSceneParameter sceneParameter,
    Single min,
    Single max) : IIsfParameter
{
    private Single Min { get; } = min;

    private Single Max { get; } = max;

    public Single CalculateValue()
    {
        var distance = Max - Min;
        var coefficient = (Single)OpenGlSceneParameter.Value / Byte.MaxValue;
        var valueOffset = coefficient * distance;
        var result = Min + valueOffset;
        return result;
    }

    private OpenGlSceneParameter OpenGlSceneParameter { get; } = sceneParameter;

    public string Type => OpenGlConstants.ParameterTypes.Float;
}