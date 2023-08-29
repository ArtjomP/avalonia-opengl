#nullable enable

using Silk.NET.OpenGL;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Silk;

public class BufferObject<TDataType> : IDisposable
       where TDataType : unmanaged
{
    private UInt32 _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;

        _handle = _gl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(
                bufferType,
                (UIntPtr)(data.Length * sizeof(TDataType)),
                d, 
                BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}