using Avalonia.OpenGL;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System;
using System.Linq;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using CommunityToolkit.Diagnostics;
using Avalonia.PixelColor.Utils.OpenGl.ShaderToy;


namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

public sealed class ShaderToyScene : IOpenGlScene 
{
    private const String VertexShaderSource =
        @"
#version 150
precision highp float;
void main()
{
    gl_Position = vec4(float((gl_VertexID&1)<<2)-1.0, float((gl_VertexID&2)<<1)-1.0, 0, 1);
}";

    private readonly String _fragmentShaderSource;

    private readonly ShaderToyUniforms _uniforms;
    
    private UInt32 _noiseTexture;
    
    private IEnumerable<IsfSceneParameterOfSingle> _isfParameters;

    public ShaderToyScene(
        String fragmentShaderSource)
    {
        Guard.IsNotNullOrEmpty(fragmentShaderSource);
        _fragmentShaderSource = ShaderToyConverter.Convert(fragmentShaderSource);
        _uniforms = new ShaderToyUniforms();

        _isfParameters = [];
        Parameters = [];
    }

    private GL? _gl;
    
    private UInt32 _vao;
    
    private Silk.Shader? _shader;

    public IEnumerable<OpenGlSceneParameter> Parameters { get; private set; }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ShaderToy;

    public void DeInitialize(GlInterface gl)
    {
        _gl?.DeleteVertexArray(_vao);
        _shader?.Dispose();
        _gl?.DeleteTexture(_noiseTexture);
    }

    public void Initialize(GlInterface gl)
    {
        Guard.IsNotNull(gl);

        _gl = GL.GetApi(gl.GetProcAddress);
        _vao = _gl.GenVertexArray();

        _shader = new Silk.Shader(
            gl: _gl,
            vertexPath: VertexShaderSource,
            fragmentPath: _fragmentShaderSource,
            loadShadersFromFile: false);

        _noiseTexture = _gl.GenTexture();
        _gl.BindTexture(GLEnum.Texture2D, _noiseTexture);

        var random = new Random(3);
        var noise = new Byte[256 * 256 * 4];
        for (Int32 i = 0; i < noise.Length; i++)
        {
            noise[i] = (Byte)(random.Next() & 0xff);
        }

        unsafe
        {
            fixed (Byte* pNoise = noise)
            {
                _gl.TexImage2D(
                    target: TextureTarget.Texture2D,
                    level: 0,
                    internalformat: InternalFormat.Rgba8,
                    width: 256,
                    height: 256,
                    border: 0,
                    format: PixelFormat.Rgba,
                    type: PixelType.UnsignedByte,
                    pixels: pNoise);
            }
        }

        // linear, wrap
        _gl.TexParameter(
            target: GLEnum.Texture2D,
            pname: GLEnum.TextureMinFilter, 
            param: (Int32)TextureMinFilter.Linear);
        _gl.TexParameter(
            target: GLEnum.Texture2D,
            pname: GLEnum.TextureMagFilter, 
            param: (Int32)TextureMinFilter.Linear);
        _gl.TexParameter(
            target: GLEnum.Texture2D,
            pname: GLEnum.TextureWrapS,
            param: (Int32)TextureWrapMode.Repeat);
        _gl.TexParameter(
            target: GLEnum.Texture2D,
            pname: GLEnum.TextureWrapT, 
            param: (Int32)TextureWrapMode.Repeat);
        _gl.BindTexture(
            target: GLEnum.Texture2D, 
            texture: 0);

        _isfParameters = _uniforms.FindParameters(
            shaderProgram: _shader.Handle,
            gl: _gl);

        Parameters = _isfParameters.Select(e => e.OpenGlSceneParameter).ToList();
        
        _uniforms.startMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        _uniforms.lastMs = _uniforms.startMs;
        _uniforms.frame = 0;
    }

    public void Render(GlInterface oldGl, Int32 width, Int32 height)
    {
        var gl = _gl ?? GL.GetApi(oldGl.GetProcAddress);
        gl.ClearColor(System.Drawing.Color.Black);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Enable(EnableCap.DepthTest);
        var shader = _shader;
        if (shader is not null)
        {
            shader.Use();
            _uniforms.SetUniforms(_isfParameters);
            var now = DateTime.Now;
            Int64 currentMillisecond = now.Ticks / TimeSpan.TicksPerMillisecond;
            Single timeDelta = (currentMillisecond - _uniforms.lastMs) / 1000f;
            
            if (_uniforms.iTime >= 0) gl.Uniform1(_uniforms.iTime, (currentMillisecond - _uniforms.startMs) / 1e3f);
            if (_uniforms.iResolution >= 0) gl.Uniform3(_uniforms.iResolution, width, height, 1.0f);
            if (_uniforms.iTimeDelta >= 0) gl.Uniform1(_uniforms.iTimeDelta, timeDelta);
            if (_uniforms.iFrameRate >= 0) gl.Uniform1(_uniforms.iTimeDelta, 1.0f / timeDelta);
            if (_uniforms.iFrame >= 0) gl.Uniform1(_uniforms.iFrame, _uniforms.frame);
            if (_uniforms.iChannelTime >= 0) gl.Uniform1(_uniforms.iChannelTime, new ReadOnlySpan<float>([0, 0, 0, 0]));
            if (_uniforms.iChannelResolution >= 0)
                gl.Uniform3(_uniforms.iChannelResolution,
                    new ReadOnlySpan<float>([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]));
            if (_uniforms.iMouse >= 0) gl.Uniform4(_uniforms.iMouse, 0f, 0f, 0f, 0f);
            if (_uniforms.iDate >= 0)
                gl.Uniform4(_uniforms.iDate, now.Year, now.Month, now.Day,
                    now.Hour * 3600f + now.Minute * 60f + now.Second);
            
            if (_uniforms.iAudioLow >= 0) gl.Uniform1(_uniforms.iAudioLow, _uniforms.audioLow);
            if (_uniforms.iForce >= 0) gl.Uniform1(_uniforms.iForce, _uniforms.force);
            if (_uniforms.iForce2 >= 0) gl.Uniform1(_uniforms.iForce2, _uniforms.force2);
            if (_uniforms.iForce3 >= 0) gl.Uniform1(_uniforms.iForce3, _uniforms.force3);
            if (_uniforms.iComplexity >= 0) gl.Uniform1(_uniforms.iComplexity, _uniforms.complexity);
            if (_uniforms.iNbItems >= 0) gl.Uniform1(_uniforms.iNbItems, _uniforms.nbItems);
            if (_uniforms.iNbItems2 >= 0) gl.Uniform1(_uniforms.iNbItems2, _uniforms.nbItems2);
            if (_uniforms.mColorMode >= 0) gl.Uniform1(_uniforms.mColorMode, _uniforms.colorMode);
            
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, _noiseTexture);
            if (_uniforms.iChannel0 >= 0) gl.Uniform1(_uniforms.iChannel0, 0);
            if (_uniforms.iChannel1 >= 0) gl.Uniform1(_uniforms.iChannel1, 0);
            if (_uniforms.iChannel2 >= 0) gl.Uniform1(_uniforms.iChannel2, 0);
            if (_uniforms.iChannel3 >= 0) gl.Uniform1(_uniforms.iChannel3, 0);
            if (_uniforms.iAudioFFT >= 0) gl.Uniform1(_uniforms.iAudioFFT, 0);
            if (_uniforms.iAudioSamples >= 0) gl.Uniform1(_uniforms.iAudioSamples, 0);

            _uniforms.lastMs = currentMillisecond;
            _uniforms.frame++;

            gl.Disable(GLEnum.CullFace);
            gl.Disable(GLEnum.DepthTest);
            gl.BindVertexArray(_vao);
            gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
            gl.UseProgram(0);
        }
    }
}