using Avalonia.PixelColor.Utils.OpenGl;
using System;

namespace Avalonia.PixelColor.Models;

public sealed class SceneWithFragmentShaderDescription(
    OpenGlScenesEnum glScenesEnum,
    String name,
    String fragmentShader) : ISceneDescription
{
    public String Name { get; } = name;

    public String FragmentShader { get; } = fragmentShader;

    public OpenGlScenesEnum GlScenesEnum { get; } = glScenesEnum;
}