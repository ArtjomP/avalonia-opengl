
using System.Collections.Generic;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

public interface IShaderToyFragmentShaderConverter
{
    String ConvertToOpenGlFragmentShader(String shaderToyFragmentShader);

    String FindMainImage(String shaderToyFragmentShader);

    IEnumerable<String> GetParameters(String mainFunctionString);
}