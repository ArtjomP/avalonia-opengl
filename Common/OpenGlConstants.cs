using System;

namespace Common;

public static class OpenGlConstants
{
    public static readonly Single[] Vertices =
    {
        0.85f,  0.85f, 0.0f,
        0.85f, -0.85f, 0.0f,
        -0.85f, -0.85f, 0.0f,
        -0.85f,  0.85f, 0.0f
    };

    public static readonly UInt32[] Indices =
    {
        0u, 1u, 3u,
        1u, 2u, 3u
    };

    public const Int32 RgbaSize = 4;
    
    public const int GL_LINES = 0x0001;
    
    public const int GL_UNSIGNED_INT = 0x1405;

    public const int GL_COLOR_ATTACHMENT0 = 0x8CE0;
}