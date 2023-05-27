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
        using var image = new Image<Rgba32>(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x * pixelSize;
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