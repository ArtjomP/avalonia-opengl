using Avalonia.OpenGL;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.Visualization;
using CommunityToolkit.Diagnostics;
using Avalonia.PixelColor.Utils.OpenGl.ShaderToy;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;

namespace Avalonia.PixelColor.Utils.OpenGl.Scenes;

public sealed class ShaderToyScene : IOpenGlScene {
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
    private UInt32 _audioTexture;
    private byte[] _audioTextureData = new byte[4096];

    private float[] _fftBuffer = new float[2048];
    private byte[] _currFftBuffer = new byte[2048];

    private WasapiCapture? _soundIn;
    private ISoundOut? _soundOut;
    private IWaveSource? _source;
    private PitchShifter _pitchShifter;
    private BasicSpectrumProvider? _spectrumProvider;

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

    public bool HasAudio
    {
        get => _uniforms.iAudioFFT >= 0 || _uniforms.iAudioSamples >= 0;
    }

    public OpenGlScenesEnum Scene => OpenGlScenesEnum.ShaderToy;

    public void DeInitialize(GlInterface gl)
    {
        _gl?.DeleteVertexArray(_vao);
        _shader?.Dispose();
        _gl?.DeleteTexture(_noiseTexture);
        AudioStop();
    }

    public void Initialize(GlInterface gl)
    {
        UseSpeakerCapture();

        Guard.IsNotNull(gl);

        _gl = GL.GetApi(gl.GetProcAddress);
        _vao = _gl.GenVertexArray();

        _shader = new Silk.Shader(
            gl: _gl,
            vertexPath: VertexShaderSource,
            fragmentPath: _fragmentShaderSource,
            loadShadersFromFile: false);

        _noiseTexture = _gl.GenTexture();
        _audioTexture = _gl.GenTexture();
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

        _gl.BindTexture(GLEnum.Texture2D, _audioTexture);

        unsafe
        {
            fixed (Byte* pData = _audioTextureData)
            {
                _gl.TexImage2D(
                    target: TextureTarget.Texture2D,
                    level: 0,
                    internalformat: InternalFormat.R8,
                    width: 512,
                    height: 2,
                    border: 0,
                    format: PixelFormat.Red,
                    type: PixelType.UnsignedByte,
                    pixels: pData);
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
            param: (Int32)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(
            target: GLEnum.Texture2D,
            pname: GLEnum.TextureWrapT,
            param: (Int32)TextureWrapMode.ClampToEdge);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _isfParameters = _uniforms.FindParameters(
            shaderProgram: _shader.Handle,
            gl: _gl);

        Parameters = _isfParameters.Select(e => e.OpenGlSceneParameter).ToList();

        _uniforms.startMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        _uniforms.lastMs = _uniforms.startMs;
        _uniforms.frame = 0;
    }


    private void AudioStop()
    {
        if (_soundOut != null)
        {
            _soundOut.Stop();
            _soundOut.Dispose();
            _soundOut = null;
        }

        if (_soundIn != null)
        {
            _soundIn.Stop();
            _soundIn.Dispose();
            _soundIn = null;
        }

        if (_source != null)
        {
            _source.Dispose();
            _source = null;
        }
    }

    private void SetupSampleSource(ISampleSource aSampleSource)
    {
        _spectrumProvider = new BasicSpectrumProvider(aSampleSource.WaveFormat.Channels, aSampleSource.WaveFormat.SampleRate, FftSize.Fft2048);
        var notificationSource = new SingleBlockNotificationStream(aSampleSource);
        notificationSource.SingleBlockRead += (s, a) => _spectrumProvider.Add(a.Left, a.Right);
        _source = notificationSource.ToWaveSource(16);
    }

    public void UseSpeakerCapture()
    {
        try
        {
            AudioStop();

            _soundIn = new WasapiLoopbackCapture();
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out _pitchShifter);

            SetupSampleSource(source);

            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, aEvent) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
            };

            _soundIn.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AudioStop();
        }
    }

    public void UseMicrophoneCapture()
    {
        try
        {
            AudioStop();

            _soundIn = new WasapiCapture();
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out _pitchShifter);

            SetupSampleSource(source);

            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, aEvent) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
            };

            _soundIn.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AudioStop();
        }
    }

    public void UseAudioFile(String file)
    {
        try
        {
            AudioStop();

            //open the selected file
            ISampleSource source = CodecFactory.Instance.GetCodec(file)
                .ToSampleSource()
                .AppendSource(x => new PitchShifter(x), out _pitchShifter);

            SetupSampleSource(source);

            _soundOut = new WasapiOut();
            _soundOut.Initialize(_source);
            _soundOut.Play();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AudioStop();
        }
    }

    public void UpdateAudioTexture(Single[] fftBuffer)
    {
        GL? gl = _gl;
        if (gl is not null)
        {
            for (Int32 i = 0; i < fftBuffer.Length; i++)
            {
                Byte val = (Byte)Math.Clamp(_fftBuffer[i] * 10000.0f, 0, 255);
                _currFftBuffer[i] = val;
            }
        }
    }


    private void UpdateAudioTexture(GL gl)
    {
        BasicSpectrumProvider? spectrumProvider = _spectrumProvider;
        if (spectrumProvider is not null)
        {
          //  if (spectrumProvider.GetFftData(_fftBuffer, this))
          //  {
           //     for (int i = 0; i < 512; i++)
           //     {
           //         Byte val = (Byte)Math.Clamp(_fftBuffer[i] * 10000.0f, 0, 255);
            //        _currFftBuffer[i] = val;
            //    }
          //  }

            for (Int32 i = 0; i < 2048; i++)
            {
                _audioTextureData[i] = (byte)(Math.Clamp(_audioTextureData[i] * 0.8 + _currFftBuffer[i] * 0.2, 0, 255));
                _audioTextureData[i + 2048] = _audioTextureData[i];
            }

            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(
                target: GLEnum.Texture2D,
                _audioTexture);
            unsafe
            {
                fixed (Byte* pData = _audioTextureData)
                {
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 2048, 2, PixelFormat.Red, PixelType.UnsignedByte,
                        pData);
                }
            }
        }
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

            UpdateAudioTexture(gl);
            if (_uniforms.iAudioFFT >= 0) gl.Uniform1(_uniforms.iAudioFFT, 1);
            if (_uniforms.iAudioSamples >= 0) gl.Uniform1(_uniforms.iAudioSamples, 1);

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