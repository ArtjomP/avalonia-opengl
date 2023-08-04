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

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, Int32, Single*, void>)procAddress;
        functionDelegate(location, count, value);
    }

    public static unsafe void Begin(this GlInterface glInterface, Int32 mode)
    {
        const string EntryPoint = "glBegin";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, void>)procAddress;
        functionDelegate(mode);
    }

    public static unsafe void End(this GlInterface glInterface)
    {
        const string EntryPoint = "glEnd";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<void>)procAddress;
        functionDelegate();
    }

    public static unsafe void Vertex2f(this GlInterface glInterface, Single x, Single y)
    {
        const string EntryPoint = "glBegin";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Single, Single, void>)procAddress;
        functionDelegate(x, y);
    }
}