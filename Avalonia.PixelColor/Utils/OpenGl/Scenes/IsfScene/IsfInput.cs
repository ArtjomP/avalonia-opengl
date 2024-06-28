using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfInput
{

    public String NAME { get; set; } = String.Empty;

    public String TYPE { get; set; } = String.Empty;

    public Object? DEFAULT { get; set; }

    public Object? MIN { get; set; }

    public Object? MAX { get; set; }
}