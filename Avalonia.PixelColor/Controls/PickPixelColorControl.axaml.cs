using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.PixelColor.Utils.OpenGl;
using System;
using System.Collections.Generic;

namespace Avalonia.PixelColor.Controls;


public class PickPixelColorControl
    : TemplatedControl
    , IScreenShotControl
{
    public Double ScaleFactor { get; set; } = 1;

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

    public static readonly DirectProperty<PickPixelColorControl, OpenGlSceneDescription?> SceneDescriptionProperty =
       AvaloniaProperty.RegisterDirect<PickPixelColorControl, OpenGlSceneDescription?>(
           nameof(SceneDescription),
           o => o.SceneDescription,
           (o, v) => o.SceneDescription = v);

    private OpenGlSceneDescription? _sceneDescription = null;

    public OpenGlSceneDescription? SceneDescription
    {
        get => _sceneDescription;
        private set => SetAndRaise(SceneDescriptionProperty, ref _sceneDescription, value);
    }

    public static readonly DirectProperty<PickPixelColorControl, OpenGlScenesEnum> SceneProperty =
        AvaloniaProperty.RegisterDirect<PickPixelColorControl, OpenGlScenesEnum>(
            nameof(Scene),
            o => o.Scene,
            (o, v) => o.Scene = v,
            unsetValue: OpenGlScenesEnum.Rectangle);

    private OpenGlScenesEnum _scene = OpenGlScenesEnum.Rectangle;

    public OpenGlScenesEnum Scene
    {
        get => _scene;
        private set => SetAndRaise(SceneProperty, ref _scene, value);
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

    protected override void OnPropertyChanged<T>(
        AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SceneProperty)
        {
            var scene = change
                .NewValue
                .GetValueOrDefault<OpenGlScenesEnum>();
            ChangeScene(scene);
        }
    }

    private void ChangeScene(OpenGlScenesEnum scene)
    {
        var openGlControl = _openGlControl;
        if (openGlControl is not null)
        {
            var sceneParameters = openGlControl.ChangeScene(scene);
            var sceneDescription = new OpenGlSceneDescription()
            {
                Scene = scene,
                Parameters = sceneParameters
            };
            SceneDescription = sceneDescription;
            RenderOpenGl();
        }
    }

    private void TrackMoved(
       Object? sender,
       PointerEventArgs args)
    {
        var openGlControl = _openGlControl;
        if (openGlControl is not null)
        {
            var position = args.GetCurrentPoint(openGlControl);
            var width = openGlControl.Bounds.Width;
            var height = openGlControl.Bounds.Height;
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

                RenderOpenGl();
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

    private void RenderOpenGl()
    {
        var openGlControl = _openGlControl;
        if (openGlControl is not null)
        {
            openGlControl.ScaleFactor = ScaleFactor;
            openGlControl.InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        RenderOpenGl();
    }

    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        RenderOpenGl();
    }

    public void MakeScreenShot(String fullname)
    {
        _openGlControl?.MakeScreenShot(fullname);
        RenderOpenGl();
    }
}