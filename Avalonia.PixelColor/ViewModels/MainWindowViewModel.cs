using Avalonia.PixelColor.Controls;
using Avalonia.PixelColor.Utils.OpenGl;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Avalonia.PixelColor.ViewModels;

public sealed class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        Scenes = (OpenGlScenesEnum[])Enum.GetValues(typeof(OpenGlScenesEnum));
        SelectedScene = OpenGlScenesEnum.Lines;
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

    private void SetShowEditorButtonVisible(OpenGlScenesEnum scene)
    {
        ShowEditorButtonVisible = scene == OpenGlScenesEnum.IsfScene;
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

    public IEnumerable<OpenGlScenesEnum> Scenes { get; }

    [Reactive]
    public OpenGlScenesEnum SelectedScene
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
    } = new ObservableCollection<OpenGlSceneParameter>();

    [Reactive]
    public IScreenShotControl? ScreenShotControl { get; set; }

    [Reactive]
    public String ScreenShotsFolder { get; set; } = String.Empty;

    [Reactive]
    public String Error { get; set; } = String.Empty;
}