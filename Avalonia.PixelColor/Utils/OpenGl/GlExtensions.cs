using Avalonia.OpenGL;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public static class GlExtensions
{
    public static unsafe void Uniform3fv(this GlInterface glInterface, Int32 location, Int32 count, Single* value)
    {
        const string EntryPoint = "glUniform3fv";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var uniform3fDelegate = (delegate* unmanaged[Stdcall]<Int32, Int32, Single*, void>)procAddress;
        uniform3fDelegate(location, count, value);
    }
}