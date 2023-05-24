using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Avaloina.PixelColor.Controls;

public class PickPixelColorControl 
    : TemplatedControl
    , IScreenShotControl
{
    private OpenGlControl? _openGlControl;

    public static readonly DirectProperty<PickPixelColorControl, IEnumerable<TrackPointViewModel>> TrackPointsProperty =
        AvaloniaProperty.RegisterDirect<PickPixelColorControl, IEnumerable<TrackPointViewModel>>(
            nameof(TrackPoints),
            o => o.TrackPoints,
            (o, v) => o.TrackPoints = v);

    private IEnumerable<TrackPointViewModel> _trackPoints = new List<TrackPointViewModel>()
    {
        new TrackPointViewModel(),
    };

    public IEnumerable<TrackPointViewModel> TrackPoints
    {
        get { return _trackPoints; }
        set { SetAndRaise(TrackPointsProperty, ref _trackPoints, value); }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var openGlControl = e.NameScope.Find<OpenGlControl>("PART_OpenGl");
        if (openGlControl is not null)
        {
            foreach (var trackPoint in TrackPoints)
            {
                openGlControl.TrackPoints.Add(trackPoint.Point);
            }
        }

        _openGlControl = openGlControl;
        PointerMovedEvent.AddClassHandler<PickPixelColorControl>(TrackMoved);
    }

    private void TrackMoved(
       Object? sender,
       PointerEventArgs args)
    {
        var openGlControl = _openGlControl;
        if (openGlControl is not null)
        {
            var position = args.GetCurrentPoint(openGlControl);
            var width = Bounds.Width;
            var height = Bounds.Height;
            if (position is not null)
            {
                var x = position.Position.X;
                var y = position.Position.Y;
                var relativeX = x / width;
                var relativeY = y / height;
                foreach (var trackPoint in TrackPoints)
                {
                    trackPoint.RelativeX = relativeX;
                    trackPoint.RelativeY = relativeY;
                    trackPoint.Point.ReleativeX = relativeX;
                    trackPoint.Point.ReleativeY = relativeY;
                }

                openGlControl?.InvalidateVisual();

                foreach (var trackPoint in TrackPoints)
                {
                    var color = trackPoint.Point.Color;
                    var mediaColor = Color.FromArgb(
                        a: color.A,
                        r: color.R,
                        g: color.G,
                        b: color.B);
                    var brush = new SolidColorBrush(mediaColor);
                    trackPoint.ColorBrush = brush;
                }
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        _openGlControl?.InvalidateVisual();
    }

    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        _openGlControl?.InvalidateMeasure();
    }

    public void MakeScreenShot(String fullname)
    {
        _openGlControl?.MakeScreenShot(fullname);
        _openGlControl?.InvalidateVisual();
    }
}