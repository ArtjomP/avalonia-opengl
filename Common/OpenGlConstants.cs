﻿using System;

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
}