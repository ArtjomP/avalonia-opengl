using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avaloina.PixelColor.Controls;

public class PickPixelColorControl : TemplatedControl
{
    private OpenGlControl? _openGlControl;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _openGlControl = e.NameScope.Find<OpenGlControl>("PART_OpenGl");
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        _openGlControl?.Render(context);
    }

    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        _openGlControl?.InvalidateMeasure();
    }
}