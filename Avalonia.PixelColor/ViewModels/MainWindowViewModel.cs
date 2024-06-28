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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Avalonia.PixelColor.ViewModels;

public sealed class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        AddShaderToySceneCommand = ReactiveCommand.CreateFromTask(AddShaderToySceneAsync);
        Scenes = Enum
            .GetValues<OpenGlScenesEnum>()
            .Select(a => new DefaultSceneDescription(a));
        SelectedScene = Scenes.First(o => o.GlScenesEnum is OpenGlScenesEnum.ColorfulVoronoi);
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

    private void SetShowEditorButtonVisible(ISceneDescription scene)
    {
        ShowEditorButtonVisible = scene.GlScenesEnum is OpenGlScenesEnum.IsfScene;
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
}