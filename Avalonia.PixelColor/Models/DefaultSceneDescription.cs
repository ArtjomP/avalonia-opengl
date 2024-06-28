using Avalonia.PixelColor.Utils.OpenGl;
using System;

namespace Avalonia.PixelColor.Models;

public sealed class DefaultSceneDescription : ISceneDescription
{
    public DefaultSceneDescription(OpenGlScenesEnum scene)
    {
        Name = scene.ToString();
        GlScenesEnum = scene;
    }

    public String Name { get; }

    public OpenGlScenesEnum GlScenesEnum { get; }
}