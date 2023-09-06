#nullable enable

using CommunityToolkit.Diagnostics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public sealed class OpenGlSceneParameter : ReactiveObject
{
    public OpenGlSceneParameter(String name)
        : this(name, 0)
    {
    }

    public OpenGlSceneParameter(
        String name,
        Byte value,
        Byte minimum = 0,
        Byte maximum = Byte.MaxValue)
    {
        Guard.IsNotNullOrEmpty(name);
        Name = name;
        Value = value;
        Minimum = minimum;
        Maximum = maximum;
    }

    public String Name
    {
        get;
    }

    [Reactive]
    public Byte Value
    {
        get;
        set;
    }

    public Byte Minimum { get; set; }
    public Byte Maximum { get; set; }
}