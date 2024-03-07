using Avalonia.OpenGL;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl;

public static class GlExtensions
{
    public static unsafe void Uniform3fv(this GlInterface glInterface, Int32 location, Int32 count, Single* value)
    {
        const String EntryPoint = "glUniform3fv";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, Int32, Single*, void>)procAddress;
        functionDelegate(location, count, value);
    }

    public static unsafe void Uniform2f(this GlInterface glInterface, Int32 location, Single v0, Single v1)
    {
        const String EntryPoint = "glUniform2f";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, Single, Single, void>)procAddress;
        functionDelegate(location, v0, v1);
    }

    public static unsafe void Begin(this GlInterface glInterface, Int32 mode)
    {
        const String EntryPoint = "glBegin";
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
        const String EntryPoint = "glEnd";
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
        const String EntryPoint = "glVertex2f";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Single, Single, void>)procAddress;
        functionDelegate(x, y);
    }

    public static unsafe void LineWidth(this GlInterface glInterface, Single width)
    {
        const String EntryPoint = "glLineWidth";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Single, void>)procAddress;
        functionDelegate(width);
    }

    public static unsafe void Color3f(this GlInterface glInterface, Single red, Single green, Single blue)
    {
        const String EntryPoint = "glColor3f";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Single, Single, Single, void>)procAddress;
        functionDelegate(red, green, blue);
    }

    public static unsafe void Color3d(this GlInterface glInterface, Double red, Double green, Double blue)
    {
        const String EntryPoint = "glColor3d";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Double, Double, Double, void>)procAddress;
        functionDelegate(red, green, blue);
    }

    public static unsafe void LoadIdentity(this GlInterface glInterface)
    {
        const String EntryPoint = "glLoadIdentity";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<void>)procAddress;
        functionDelegate();
    }

    public static unsafe void PushMatrix(this GlInterface glInterface)
    {
        const String EntryPoint = "glPushMatrix";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<void>)procAddress;
        functionDelegate();
    }

    public static unsafe void SwapBuffers(this GlInterface glInterface, IntPtr hDC)
    {
        const String EntryPoint = "wglSwapBuffers";
        var procAddress = glInterface.GetProcAddress(EntryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + EntryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<IntPtr, void>)procAddress;
        functionDelegate(hDC);
    }

    public static unsafe void MatrixMode(this GlInterface glInterface, Int32 mode)
    {
        const String entryPoint = "glMatrixMode";
        var procAddress = glInterface.GetProcAddress(entryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + entryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, void>)procAddress;
        functionDelegate(mode);
    }

    public static unsafe void Ortho(this GlInterface glInterface, Int32 x, Int32 w, Int32 h, Int32 y, Single n, Single f)
    {
        const String entryPoint = "glOrtho";
        var procAddress = glInterface.GetProcAddress(entryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + entryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, Int32, Int32, Int32, Single, Single, void>)procAddress;
        functionDelegate(x, w, h, y, n, f);
    }

    public static unsafe void ReadPixels(
        this GlInterface glInterface,
        Int32 x,
        Int32 y,
        Int32 width,
        Int32 height,
        Int32 format,
        Int32 type,
        void* data)
    {
        const String entryPoint = "glReadPixels";
        var procAddress = glInterface.GetProcAddress(entryPoint);
        if (procAddress == IntPtr.Zero)
        {
            throw new ArgumentException("Entry point not found: " + entryPoint);
        }

        var functionDelegate = (delegate* unmanaged[Stdcall]<Int32, Int32, Int32, Int32, Int32, Int32, void*, void>)procAddress;
        functionDelegate(x, y, width, height, format, type, data);
    }
}