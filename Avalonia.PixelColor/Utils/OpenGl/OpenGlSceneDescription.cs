using System.Collections.Generic;
using System.Linq;

namespace Avalonia.PixelColor.Utils.OpenGl;

public sealed class OpenGlSceneDescription
{
    public OpenGlScenesEnum Scene
    {
        get;
        init;
    }

    public IEnumerable<OpenGlSceneParameter> Parameters
    {
        get;
        init;
    } = Enumerable.Empty<OpenGlSceneParameter>();
}
