using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public interface IIsfShaderParser
{
    IsfParameters GetIsfParameters(String source);

    String GetShaderCode(String source);
}