using Avalonia.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;
using Avalonia.PixelColor.Utils.OpenGl.Scenes;

namespace Avalonia.PixelColor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        PlatformImpl.ScalingChanged += o => OpenGlControl.ScaleFactor = o;
        var scaleFactor = PlatformImpl.DesktopScaling;
        OpenGlControl.ScaleFactor = scaleFactor;
        ViewModel.ScreenShotControl = OpenGlControl;
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
        var scene = (ISFScene)OpenGlControl.SelectedScene;

        txtFS.Text = scene.ISFFragmentShaderSource;
        txtVS.Text = scene.ISFVertexShaderSource;
    }

    private void OnApplyClick(Object sender, RoutedEventArgs e)
    {
        ISFScene scene = (ISFScene)OpenGlControl.SelectedScene;
        scene.SetUp(txtVS.Text, txtFS.Text);
        ViewModel.UpdateParameters(scene.Parameters);

        EditorPanel.IsVisible = false;
        ViewModel.ShowEditorButtonVisible = true;
    }
}