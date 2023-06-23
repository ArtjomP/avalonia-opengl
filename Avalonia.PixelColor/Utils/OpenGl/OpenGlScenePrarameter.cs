#nullable enable

using CommunityToolkit.Diagnostics;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public sealed class OpenGlSceneParameter
{
    public OpenGlSceneParameter(String name)
        : this(name, 0)
    {
    }

    public OpenGlSceneParameter(String name, Byte value)
    {
        Guard.IsNotNullOrEmpty(name);
        Name = name;
        Value = value;
    }

    public String Name
    {
        get;
    }

    public Byte Value
    {
        get;
        set;
    }
}
