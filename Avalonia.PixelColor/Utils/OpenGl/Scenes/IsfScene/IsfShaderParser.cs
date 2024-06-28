using Common;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfShaderParser : IIsfShaderParser
{
    public IsfParameters GetIsfParameters(String source)
    {
        String json = source.Substring("/*", "*/");
        IsfParameters parameters = JsonConvert
            .DeserializeObject<IsfParameters>(json) ??
            new IsfParameters();
        return parameters;
    }

    public String GetShaderCode(String source)
    {
        Int32 index = source.IndexOf("*/", StringComparison.Ordinal);
        String code = source.Substring(index + 2, source.Length - index - 2);

        IsfInput[] inputs = GetIsfParameters(source).INPUTS;

        var sb = new StringBuilder();
        sb.Append("out vec4 gl_FragColor;\r\n");
        sb.Append("uniform float TIME;\r\n");
        sb.Append("uniform vec2 RENDERSIZE;\r\n");
        foreach (IsfInput input in inputs)
        {
            String uniformType = input.TYPE switch
            {
                OpenGlConstants.ParameterTypes.Color => "vec4",
                OpenGlConstants.ParameterTypes.Point2d => "vec2",
                _ => input.TYPE
            };
            sb.Append($"uniform {uniformType} {input.NAME};\r\n");
        }

        sb.Append(code);

        String result = sb.ToString();
        return result;
    }
}