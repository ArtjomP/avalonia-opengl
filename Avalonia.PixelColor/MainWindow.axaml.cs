using Avalonia.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;
using Avalonia.PixelColor.Utils.OpenGl.Scenes;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Avalonia.OpenGL;

namespace Avalonia.PixelColor;

public partial class MainWindow : Window
{ 
    public MainWindow()
    {
        InitializeComponent(true, true);
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        ViewModel.ScreenShotControl = OpenGlControl;
        ScalingChanged += MainWindow_ScalingChanged;
    }

    private void MainWindow_ScalingChanged(Object? sender, EventArgs e)
    {
        var scaling = this.RenderScaling;
        SetScaleFactor(scaling);
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
        if(String.IsNullOrEmpty(txtVS.Text) &&
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
}