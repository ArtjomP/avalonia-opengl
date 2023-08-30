using Avalonia.Media;
using Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Avalonia.PixelColor;

public class TrackPointViewModel : ReactiveObject
{
    [Reactive]
    public SolidColorBrush ColorBrush { get; set; } = new SolidColorBrush();

    [Reactive]
    public Double RelativeX { get; set; }

    [Reactive]
    public Double RelativeY { get; set; }

    public TrackPoint Point { get; set; } = new TrackPoint();
}