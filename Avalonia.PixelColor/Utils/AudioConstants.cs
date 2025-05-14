using System;
using SoundIOSharp;

namespace Avalonia.PixelColor.Utils;

public class AudioConstants
{
    public static readonly SoundIOFormat[] WantedFormatsByPriority =
    [
        SoundIODevice.Float32NE,
        SoundIODevice.Float32FE,
        SoundIODevice.S32NE,
        SoundIODevice.S32FE,
        SoundIODevice.S24NE,
        SoundIODevice.S24FE,
        SoundIODevice.S16NE,
        SoundIODevice.S16FE,
        SoundIODevice.Float64NE,
        SoundIODevice.Float64FE,
        SoundIODevice.U32NE,
        SoundIODevice.U32FE,
        SoundIODevice.U24NE,
        SoundIODevice.U24FE,
        SoundIODevice.U16NE,
        SoundIODevice.U16FE,
        SoundIOFormat.S8,
        SoundIOFormat.U8,
        SoundIOFormat.Invalid,
    ];

    public static readonly Int32[] WantedSampleRatesByPriority =
    [
        44100,
        48000,
        96000,
        24000,
        0,
    ];   
}