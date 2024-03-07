using System.Text;
using Newtonsoft.Json;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;

public class IsfShaderParser
{
    public static IsfParameters GetIsfParameters(string source)
    {
        string json = source.Substring("/*", "*/");
        var parameters = JsonConvert.DeserializeObject<IsfParameters>(json);
        return parameters;
    }

    public static string GetShaderCode(string source)
    {
        int index = source.IndexOf("*/");
        string code = source.Substring(index + 2, source.Length - index - 2);

        IsfInput[] inputs = GetIsfParameters(source).INPUTS;

        StringBuilder sb = new StringBuilder();

        sb.Append("uniform float TIME;\r\n");
        sb.Append("uniform vec2 RENDERSIZE;\r\n");
        sb.Append("out vec4 gl_FragColor;\r\n");

        foreach (var input in inputs)
        {
            sb.Append($"uniform {input.TYPE} {input.NAME};\r\n");
        }

        sb.Append(code);

        return sb.ToString();
    }
}