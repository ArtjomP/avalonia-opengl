#nullable enable

using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public record GradientParameters(
    Direction direction,
    Single leftGradientWidth,
    Single rightGradientWidth)
{
}