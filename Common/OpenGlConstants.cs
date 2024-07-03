namespace Common;

public static class OpenGlConstants
{
    public static readonly Single[] Vertices =
    [
        0.85f,  0.85f, 0.0f,
        0.85f, -0.85f, 0.0f,
        -0.85f, -0.85f, 0.0f,
        -0.85f,  0.85f, 0.0f
    ];

    public static readonly Single[] VerticesFullScene =
    [
        //X    Y      Z
        -1.0f, -1.0f, 0.0f,
         1.0f, -1.0f, 0.0f,
        -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f, 0.0f
    ];

    public static readonly UInt32[] Indices =
    [
        0u, 1u, 3u,
        1u, 2u, 3u
    ];

    public const Int32 NotFoundIndex = -1;

    public const Int32 RgbaSize = 4;

    public const Int32 GL_INT = 5124;

    public const Int32 GL_UNSIGNED_INT = 5125;

    public static class ParameterTypes
    {
        public const String Float = "float";

        public const String Point2d = "point2D";

        public const String Color = "color";
    }

    public const String DefaultShaderToyFragmentShader = @"
out vec4 outColor;
void main()
{
    outColor = vec4(1.,0.,0.,.5);
}
";
}