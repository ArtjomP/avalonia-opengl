//using Avalonia.OpenGL;
//using CommunityToolkit.Diagnostics;
//using System;

//namespace Avalonia.PixelColor.Utils.OpenGl;

//internal sealed class GlExtrasInterface : GlInterfaceBase<GlInterface.GlContextInfo>
//{
//    public GlExtrasInterface(GlInterface gl)
//        : base(gl.GetProcAddress, gl.ContextInfo)
//    {
//        Guard.IsNotNull(GetTexImage);
//        Guard.IsNotNull(PixelStore);
//        Guard.IsNotNull(ReadBuffer);
//        Guard.IsNotNull(ReadPixels);
//        Guard.IsNotNull(DeleteVertexArrays);
//        Guard.IsNotNull(BindVertexArray);
//        Guard.IsNotNull(GenVertexArrays);
//    }

//    public unsafe delegate void GlGetTexImage(Int32 target, Int32 level, Int32 format, Int32 type, void* pixels);
//    [GlMinVersionEntryPoint("glGetTexImage", 3, 0)]
//    public GlGetTexImage GetTexImage { get; }

//    public unsafe delegate void GlPixelStore(Int32 parameterName, Int32 parameterValue);
//    [GlMinVersionEntryPoint("glPixelStorei", 3, 0)]
//    public GlPixelStore PixelStore { get; }

//    public unsafe delegate void GlReadBuffer(Int32 mode);
//    [GlMinVersionEntryPoint("glReadBuffer", 3, 0)]
//    public GlReadBuffer ReadBuffer { get; }

//    public unsafe delegate void GlReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, Int32 format, Int32 type, void* data);
//    [GlMinVersionEntryPoint("glReadPixels", 3, 0)]
//    public GlReadPixels ReadPixels { get; }

//    public delegate void GlDeleteVertexArrays(Int32 count, Int32[] buffers);
//    [GlMinVersionEntryPoint("glDeleteVertexArrays", 3, 0)]
//    [GlExtensionEntryPoint("glDeleteVertexArraysOES", "GL_OES_vertex_array_object")]
//    public GlDeleteVertexArrays DeleteVertexArrays { get; }

//    public delegate void GlBindVertexArray(Int32 array);
//    [GlMinVersionEntryPoint("glBindVertexArray", 3, 0)]
//    [GlExtensionEntryPoint("glBindVertexArrayOES", "GL_OES_vertex_array_object")]
//    public GlBindVertexArray BindVertexArray { get; }
//    public delegate void GlGenVertexArrays(Int32 n, Int32[] rv);

//    [GlMinVersionEntryPoint("glGenVertexArrays", 3, 0)]
//    [GlExtensionEntryPoint("glGenVertexArraysOES", "GL_OES_vertex_array_object")]
//    public GlGenVertexArrays GenVertexArrays { get; }

//    public Int32 GenVertexArray()
//    {
//        var rv = new Int32[1];
//        GenVertexArrays(1, rv);
//        return rv[0];
//    }
//}
