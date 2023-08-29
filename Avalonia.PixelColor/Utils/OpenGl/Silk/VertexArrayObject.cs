#nullable enable 

using Silk.NET.OpenGL;
using System;

namespace Avalonia.PixelColor.Utils.OpenGl.Silk;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private UInt32 _handle;
    private GL _gl;

    public VertexArrayObject(
        GL gl,
        BufferObject<TVertexType> vbo, 
        BufferObject<TIndexType>? ebo = null)
    {
        _gl = gl;

        _handle = _gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo?.Bind();
    }

    public unsafe void VertexAttributePointer(
        UInt32 index, 
        Int32 count,
        VertexAttribPointerType type, 
        UInt32 vertexSize, 
        Int32 offSet)
    {
        _gl.VertexAttribPointer(
            index,
            count,
            type,
            false,
            vertexSize * (UInt32)sizeof(TVertexType), 
            (void*)(offSet * sizeof(TVertexType)));
        _gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
    }
}