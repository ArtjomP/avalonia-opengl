using System.Text;

using Newtonsoft.Json;

namespace Avalonia.PixelColor.Utils.OpenGl
{
    public class ISFShaderParser
    {
		public static ISFParameters GetISFParameters(string source)
		{
			string json = source.Substring("/*", "*/");
			var parameters = JsonConvert.DeserializeObject<ISFParameters>(json);
			return parameters;
		}

		public static string GetShaderCode(string source)
		{
			int index = source.IndexOf("*/");
			string code = source.Substring(index + 2, source.Length - index - 2);

			ISFInput[] inputs = GetISFParameters(source).INPUTS;

            StringBuilder sb = new StringBuilder();
			sb.Append("uniform float TIME;\r\n");
            sb.Append("uniform vec2 RENDERSIZE;\r\n");
            sb.Append("#define gl_FragColor isf_FragColor\r\n");
            sb.Append("out vec4 gl_FragColor;\r\n");

            foreach (var input in inputs)
			{
				sb.Append($"uniform {input.TYPE} {input.NAME};\r\n");
			}

			sb.Append(code);

			return sb.ToString();
		}
    }
}
