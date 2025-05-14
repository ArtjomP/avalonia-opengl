using Avalonia.Controls;
using Avalonia.PixelColor.Controls;
using Avalonia.PixelColor.Models;
using Avalonia.PixelColor.Utils.OpenGl;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.PixelColor.Utils;
using NWaves.Transforms;
using NWaves.Utils;
using SoundIOSharp;

namespace Avalonia.PixelColor.ViewModels;

public sealed class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        AddShaderToySceneCommand = ReactiveCommand.CreateFromTask(AddShaderToySceneAsync);
        Scenes = Enum
            .GetValues<OpenGlScenesEnum>()
            .Select(a => new DefaultSceneDescription(a));
        SelectedScene = Scenes
            .First(o => o.GlScenesEnum is OpenGlScenesEnum.ColorfulVoronoi);
        AudioInputs = new(_audioInputs);
        _circularSub = new CircularBuffer.CircularBuffer<Double>(CircularBufferSize);
        _circularLow = new CircularBuffer.CircularBuffer<Double>(CircularBufferSize);
        _circularMid = new CircularBuffer.CircularBuffer<Double>(CircularBufferSize);
        _circularHi = new CircularBuffer.CircularBuffer<Double>(CircularBufferSize);
        _fft = new RealFft(FftSize);
        _signalBuffer = new Single[FftSize];
        _signalSpectrum = new Single[FftSize / 2 + 1];
        LoadSoundIo();
        var canExecute = this
            .WhenAnyValue(
                o => o.ScreenShotsFolder,
                o => o.ScreenShotControl)
            .Select(o => !String.IsNullOrEmpty(o.Item1) && o.Item2 is not null);
        MakeScreenShotCommand = ReactiveCommand.Create(MakeScreenShot);
        this.WhenAnyValue(o => o.SelectedSceneDescription)
            .Subscribe(SetSelectedSceneParameters);
        this.WhenAnyValue(o => o.SelectedScene)
            .Subscribe(SetShowEditorButtonVisible);
        this.WhenAnyValue(a => a.SelectedAudioInput)
            .Subscribe(SelectedAudioInputChanged);
    }

    private void LoadSoundIo()
    {
        try
        {
            _soundIo = new SoundIO();
            _soundIo.OnDevicesChange += UpdateAudioInputs;
            _soundIo.Connect();
            _soundIo.FlushEvents();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private const Int32 CircularBufferSize = 4;
    
    private const Int32 FftSize = 2048;
    
    private SoundIO? _soundIo;

    private readonly IDictionary<String, Int32> _inputDeviceOffsets = new Dictionary<String, Int32>();

    private void UpdateAudioInputs()
    {
        _audioInputs.Clear();
        _audioInputs.Add(String.Empty);
        _inputDeviceOffsets.Clear();
        SoundIO? soundIo = _soundIo;
        if (soundIo is not null)
        {
            Int32 inputDeviceCount = soundIo.InputDeviceCount;
            for (Int32 inputDeviceOffset = 0; inputDeviceOffset < inputDeviceCount; ++inputDeviceOffset)
            {
                SoundIODevice device = soundIo.GetInputDevice(
                    index: inputDeviceOffset);
                String name = device.Name;
                _inputDeviceOffsets.TryAdd(
                    key: name,
                    value: inputDeviceOffset);
                _audioInputs.Add(name);
            }
        }
    }

    public ICommand AddShaderToySceneCommand { get; }

    private async Task AddShaderToySceneAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(null);
            if (topLevel is not null)
            {
                // Start async operation to open the dialog.
                IReadOnlyList<IStorageFile> files = await topLevel
                    .StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title = "Open ShaderToy File",
                            AllowMultiple = false,
                            FileTypeFilter =
                            [
                                new FilePickerFileType("*.frag")
                            ]
                        })
                    .ConfigureAwait(true);
                foreach (IStorageFile file in files)
                {
                    await AddShaderToySceneAsync(file, cancellationToken);
                }
            }
        }
    }

    public async Task AddShaderToySceneAsync(IStorageFile file, CancellationToken cancellationToken)
    {
        String fragmentShader = await File
            .ReadAllTextAsync(file.Path.LocalPath, cancellationToken)
            .ConfigureAwait(true);
        SceneWithFragmentShaderDescription newScene = new(
            glScenesEnum: OpenGlScenesEnum.ShaderToy,
            name: Path.GetFileNameWithoutExtension(file.Name),
            fragmentShader: fragmentShader);
        Scenes = Scenes.Append(newScene);
        SelectedScene = newScene;
    }

    private void SetSelectedSceneParameters(OpenGlSceneDescription? sceneDescription)
    {
        SelectedSceneParameters.Clear();
        if (sceneDescription is not null)
        {
            foreach (var parameter in sceneDescription.Parameters)
            {
                SelectedSceneParameters.Add(parameter);
            }
        }
    }

    public void UpdateParameters(IEnumerable<OpenGlSceneParameter> parameters)
    {
        SelectedSceneParameters.Clear();
        foreach (var parameter in parameters)
        {
            SelectedSceneParameters.Add(parameter);
        }
    }

    private void SetShowEditorButtonVisible(ISceneDescription? scene)
    {
        ShowEditorButtonVisible = scene is { GlScenesEnum: OpenGlScenesEnum.IsfScene };
    }

    public ICommand MakeScreenShotCommand { get; }

    private void MakeScreenShot()
    {
        try
        {
            var screenShotControl = ScreenShotControl;
            if (screenShotControl is not null)
            {
                var folder = ScreenShotsFolder;
                Directory.CreateDirectory(folder);
                var filename = $"{Guid.NewGuid()}.bmp";
                var fullName = Path.Combine(folder, filename);
                screenShotControl.MakeScreenShot(fullName);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [Reactive]
    public bool ShowEditorButtonVisible
    {
        get;
        set;
    }

    [Reactive]
    public IEnumerable<ISceneDescription> Scenes
    {
        get;
        private set;
    }

    [Reactive]
    public ISceneDescription SelectedScene
    {
        get;
        set;
    }

    [Reactive]
    public OpenGlSceneDescription? SelectedSceneDescription
    {
        get;
        set;
    }

    [Reactive]
    public ObservableCollection<OpenGlSceneParameter> SelectedSceneParameters 
    {     
        get;
        set;
    } = [];

    [Reactive]
    public IScreenShotControl? ScreenShotControl { get; set; }

    [Reactive]
    public String ScreenShotsFolder { get; set; } = String.Empty;

    [Reactive]
    public String Error { get; set; } = String.Empty;

    private readonly ObservableCollection<String> _audioInputs = [];

    public ReadOnlyObservableCollection<String> AudioInputs
    {
        get;
    }

    [Reactive]
    public String SelectedAudioInput
    {
        get;
        set;
    } = String.Empty;
    
    private static readonly Double MICROPHONE_LATENCY = 0.016;

    private void SelectedAudioInputChanged(String inputName)
    {
        try
        {
            SoundIO? soundIo = _soundIo;
            if (soundIo is not null && !String.IsNullOrEmpty(inputName) && _audioInputs.Contains(inputName))
            {
                Int32 deviceOffset = _inputDeviceOffsets[inputName];
                SoundIODevice input = soundIo.GetInputDevice(index: deviceOffset);
                
                _listening = false;
                Thread.Sleep(millisecondsTimeout: 100);

                StopPreviousStream();
                
                SoundIOFormat fmt = AudioConstants
                    .WantedFormatsByPriority
                    .FirstOrDefault(input.SupportsFormat);
                if (fmt == default)
                {
                    Debug.WriteLine("Audio Input - Incompatible sample formats for " + input.Id); // panic()
                    return;
                }
                
                Int32 sampleRate = AudioConstants
                    .WantedSampleRatesByPriority
                    .FirstOrDefault(input.SupportsSampleRate);
                if (sampleRate == default)
                {
                    Debug.WriteLine("Audio Input - Incompatible sample rates for " + input.Id); // panic()
                    return;
                }
                
                _inStream = input.CreateInStream();
                _inStream.Format = fmt;
                _inStream.SampleRate = sampleRate;
                _inStream.SoftwareLatency = MICROPHONE_LATENCY;
                _inStream.ReadCallback = (fmin, fmax) => InputReadCallback(_inStream, fmin, fmax);
                _inStream.OverflowCallback = () => Debug.WriteLine("Instream OverflowCallback called");
                _inStream.ErrorCallback = () =>
                {
                    Debug.WriteLine("Sound stream ERROR");
                };
                    
                _inStream.Open();
                Thread.Sleep(millisecondsTimeout: 500);
                Int32 capacity = (Int32)(MICROPHONE_LATENCY * 2 * _inStream.SampleRate * _inStream.BytesPerFrame);
                Debug.WriteLine($"RingBuffer capacity: {capacity}");
                _ringBuffer = soundIo.CreateRingBuffer(capacity);

                _inStream.Start();
                _listening = true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private void StopPreviousStream()
    {
        try
        {
            if (_inStream != null)
            {
                _inStream.Pause(true);
                _inStream.Dispose();
                _inStream = null;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private SoundIORingBuffer? _ringBuffer = null;
    private SoundIOInStream? _inStream = null;
    private Boolean _listening = true;
    
    private readonly Single _gainFloat = 1.0f;

    /// Gain adds +-0.5 to _gainFloat, 50% == 1.0f
    private readonly Single _gainSubFloat = 1.0f;

    private readonly Single _gainLowFloat = 1.0f;
    private readonly Single _gainMidFloat = 1.0f;
    private readonly Single _gainHiFloat = 1.0f;
    
    private readonly RealFft _fft;
    private readonly Single[] _signalBuffer = [];
    private readonly Single[] _signalSpectrum = [];

    [Reactive]
    public Single[] SignalSpectrum
    {
        get;
        private set;
    } = [];
    
    private readonly Int32 _binSubFrom, _binSubTo, _binLowFrom, _binLowTo, _binMidFrom, _binMidTo, _binHiFrom, _binHiTo;

    /// Use short history of values to smooth band values
    private readonly CircularBuffer.CircularBuffer<Double> _circularSub, _circularLow, _circularMid, _circularHi;

    [Reactive]
    public Boolean IsLogarithmic
    {
        get;
        set;
    }
     private void InputReadCallback(
        SoundIOInStream instream,
        Int32 frame_count_min,
        Int32 frame_count_max)
    {
        try
        {
            SoundIORingBuffer? ringBuffer = _ringBuffer;
            if (ringBuffer is not null)
            {
                IntPtr write_ptr = ringBuffer.WritePointer;
                Int32 free_bytes = ringBuffer.FreeCount;
                Int32 chCount = instream.Layout.ChannelCount;
                Single value = 0, sumValue = 0;
                Int32 free_count = free_bytes / instream.BytesPerFrame;

                if (frame_count_min > free_count)
                {
                    Debug.WriteLine("ring buffer overflow");
                }

                Int32 frames_left = Math.Min(free_count, frame_count_max);

                Single vuPPML = 0;
                Single vuPPMR = 0;

                while (_listening)
                {
                    Int32 frame_count = frames_left;
                    SoundIOChannelAreas areas = instream.BeginRead(ref frame_count);
                    if (frame_count == 0)
                        break;

                    if (areas.IsEmpty)
                    {
                        Debug.WriteLine($"Dropped {frame_count} frames due to internal overflow");
                    }
                    else
                    {
                        for (Int32 frame = 0; frame < frame_count && _listening; ++frame)
                        {
                            sumValue = 0;
                            for (Int32 ch = 0; ch < chCount; ++ch)
                            {
                                SoundIOChannelArea area = areas.GetArea(ch);
                                unsafe
                                {
                                    Buffer.MemoryCopy(
                                        source: (void*)area.Pointer,
                                        destination: (void*)write_ptr,
                                        destinationSizeInBytes: instream.BytesPerSample,
                                        sourceBytesToCopy: instream.BytesPerSample);
                                    area.Pointer += area.Step;
                                    value = *(Single*)write_ptr;
                                }
                                value *= _gainFloat;
                                sumValue += value;
                                if (ch == 0)
                                {
                                    vuPPML = value > vuPPML ? value : vuPPML;
                                }
                                else
                                {
                                    vuPPMR = value > vuPPMR ? value : vuPPMR;
                                }
                            }

                            /// Take the highest value from all channels presented
                            _signalBuffer[frame] = sumValue;
                        }
                    }

                    instream.EndRead();
                    _fft.PowerSpectrum(
                        samples: _signalBuffer,
                        spectrum: _signalSpectrum,
                        normalize: !IsLogarithmic);
                    SignalSpectrum = [.. _signalSpectrum];

                    Single Sub = 0;
                    for (Int32 i = _binSubFrom; i < _binSubTo; ++i)
                    {
                        if (_signalSpectrum.Length > i)
                        {
                            Sub = Sub < _signalSpectrum[i]
                                ? _signalSpectrum[i]
                                : Sub;
                        }
                        else
                        {
                            Debug.WriteLine($"[Audio] Sub: {i}");
                        }
                    }

                    Double subValue = IsLogarithmic
                        ? Scale.ToDecibelPower(_gainSubFloat * Sub, 1) / 60
                        : _gainSubFloat * Sub;
                    _circularSub.PushBack(subValue);

                    Single low = 0;
                    for (Int32 i = _binLowFrom; i < _binLowTo; ++i)
                    {
                        if (_signalSpectrum.Length > i)
                        {
                            low = low < _signalSpectrum[i]
                                ? _signalSpectrum[i]
                                : low;
                        }
                        else
                        {
                            Debug.WriteLine($"[Audio] Low: {i}");
                        }
                    }

                    Double lowDecibel = IsLogarithmic
                        ? Scale.ToDecibelPower(_gainLowFloat * low, 1) / 60
                        : _gainLowFloat * low;
                    _circularLow.PushBack(lowDecibel);

                    Single Mid = 0;
                    for (Int32 i = _binMidFrom; i < _binMidTo; ++i)
                    {
                        if (_signalSpectrum.Length > i)
                        {
                            Mid = Mid < _signalSpectrum[i]
                                ? _signalSpectrum[i]
                                : Mid;
                        }
                        else
                        {
                            Debug.WriteLine($"[Audio] Mid: {i}");
                        }
                    }

                    Double midDecibel = IsLogarithmic
                        ? Scale.ToDecibelPower(_gainMidFloat * Mid, 1) / 60
                        : _gainMidFloat * Mid;
                    _circularMid.PushBack(midDecibel);

                    Single hi = 0;
                    for (Int32 i = _binHiFrom; i < _binHiTo; ++i)
                    {
                        if (_signalSpectrum.Length > i)
                        {
                            hi = hi < _signalSpectrum[i]
                                ? _signalSpectrum[i]
                                : hi;
                        }
                        else
                        {
                            Debug.WriteLine($"[Audio] Hi: {i}");
                        }
                    }

                    Double hiDecibel = IsLogarithmic
                        ? Scale.ToDecibelPower(_gainHiFloat * hi, 1) / 60
                        : _gainHiFloat * hi;
                    _circularHi.PushBack(hiDecibel);

                    frames_left -= frame_count;
                    if (frames_left <= 0)
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Audio callback error: {ex}");
        }
    }
}