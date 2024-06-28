using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfParameters
{
    public String CREDIT { get; set; } = String.Empty;

    public String DESCRIPTION { get; set; } = String.Empty;

    public String[] CATEGORIES { get; set; } = Array.Empty<String>();

    public IsfInput[] INPUTS { get; set; } = Array.Empty<IsfInput>();
}