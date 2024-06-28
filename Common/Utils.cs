using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Common;

public static class Utils
{
    public static void SaveScreenshot(
        Byte[] rgbaData,
        Int32 width,
        Int32 height,
        Int32 pixelSize,
        String filename)
    {
        using Image<Rgba32> image = new (width, height);
        for (Int32 y = 0; y < height; y++)
        {
            for (Int32 x = 0; x < width; x++)
            {
                Int32 index = y * width * pixelSize + x * pixelSize;
                Rgba32 pixel = image[x, y];
                pixel.R = rgbaData[index];
                pixel.G = rgbaData[index + 1];
                pixel.B = rgbaData[index + 2];
                pixel.A = rgbaData[index + 3];
                image[x, y] = pixel;
            }
        }

        image.SaveAsBmp(filename);
    }
}