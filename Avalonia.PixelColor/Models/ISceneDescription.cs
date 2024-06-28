using Avalonia.PixelColor.Utils.OpenGl;
using System;

namespace Avalonia.PixelColor.Models;

public interface ISceneDescription
{
    public string Name { get; }

    OpenGlScenesEnum GlScenesEnum { get; }
}