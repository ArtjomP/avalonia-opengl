using Avalonia.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading;
using Avalonia.PixelColor.Utils.OpenGl.Scenes;

namespace Avalonia.PixelColor;

public partial class MainWindow : Window {
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        ViewModel.ScreenShotControl = OpenGlControl;
        OpenGlControl.SceneParametersChanged +=
            () => ViewModel.UpdateParameters(OpenGlControl.SelectedScene.Parameters);
        ScalingChanged += MainWindow_ScalingChanged;
        SetScaleFactor(RenderScaling);
    }

    private void MainWindow_ScalingChanged(Object? sender, EventArgs e)
    {
        SetScaleFactor(RenderScaling);
    }

    private void SetScaleFactor(Double scaleFactor)
    {
        OpenGlControl.ScaleFactor = scaleFactor;
    }

    private MainWindowViewModel ViewModel { get; }

    private void OnMakeScreenshotClick(Object sender, RoutedEventArgs e)
    {
        var executor = new CommandExecutor();
        executor.Execute(ViewModel.MakeScreenShotCommand);
    }

    private void OnHideEditorClick(Object sender, RoutedEventArgs e)
    {
        EditorPanel.IsVisible = false;
        ViewModel.ShowEditorButtonVisible = true;
    }

    private void OnShowEditorClick(Object sender, RoutedEventArgs e)
    {
        EditorPanel.IsVisible = true;
        ViewModel.ShowEditorButtonVisible = false;
        var scene = (IsfScene)OpenGlControl.SelectedScene;
    }

    private void OnApplyClick(Object sender, RoutedEventArgs e)
    {
        if (String.IsNullOrEmpty(txtVS.Text) &&
            String.IsNullOrEmpty(txtFS.Text))
        {
            return;
        }

        if (OpenGlControl.SelectedScene is IsfScene scene)
        {
            var newScene = new IsfScene(
                glVersion: OpenGlControl.GlVersion,
                fragmentShaderSource: txtFS.Text);
            OpenGlControl.ChangeScene(newScene);
            ViewModel.UpdateParameters(scene.Parameters);
        }

        EditorPanel.IsVisible = false;
        ViewModel.ShowEditorButtonVisible = true;
    }

    private async void Button_Click(
        Object? sender, RoutedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
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
                    })
                .ConfigureAwait(true);
            foreach (IStorageFile file in files)
            {
                await ViewModel
                    .AddShaderToySceneAsync(file, CancellationToken.None)
                    .ConfigureAwait(true);
            }
        }
    }

    private async void OpenAudioFile_Click(Object? sender, RoutedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not null)
        {
            IReadOnlyList<IStorageFile> files = await topLevel
                .StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Open Audio File",
                        AllowMultiple = false,
                    })
                .ConfigureAwait(true);

            var file = files.Count > 0 ? files[0] : null;

            if (file != null && OpenGlControl.SelectedScene is ShaderToyScene)
            {
                ((ShaderToyScene)OpenGlControl.SelectedScene).UseAudioFile(file.Path.AbsolutePath);
            }
        }
    }

    private async void UseSpeakerCapture_Click(Object? sender, RoutedEventArgs e)
    {
        if (OpenGlControl.SelectedScene is ShaderToyScene)
        {
            ((ShaderToyScene)OpenGlControl.SelectedScene).UseSpeakerCapture();
        }
    }

    private async void UseMicrophoneCapture_Click(Object? sender, RoutedEventArgs e)
    {
        if (OpenGlControl.SelectedScene is ShaderToyScene)
        {
            ((ShaderToyScene)OpenGlControl.SelectedScene).UseMicrophoneCapture();
        }
    }
}