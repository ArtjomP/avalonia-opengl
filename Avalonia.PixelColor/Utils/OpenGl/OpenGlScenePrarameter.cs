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
        Byte value)
    {
        Guard.IsNotNullOrEmpty(name);
        Name = name;
        Value = value;
        Minimum = Byte.MinValue;
        Maximum = Byte.MaxValue;
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

    public Byte Minimum { get; } 

    public Byte Maximum { get; }
}