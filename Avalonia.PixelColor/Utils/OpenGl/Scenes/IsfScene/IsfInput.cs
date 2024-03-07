#nullable enable

using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfInput
{
    public String NAME { get; set; } = String.Empty;

    public String TYPE { get; set; } = String.Empty;

    public Single DEFAULT { get; set; }

    public Single MIN { get; set; }

    public Single MAX { get; set; }
}

public class IsfParameters
{
    public String CREDIT { get; set; } = String.Empty;

    public String DESCRIPTION { get; set; } = String.Empty;

    public String[] CATEGORIES { get; set; } = Array.Empty<String>();

    public IsfInput[] INPUTS { get; set; } = Array.Empty<IsfInput>();
}
