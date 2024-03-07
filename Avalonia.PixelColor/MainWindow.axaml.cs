using Avalonia.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;
using Avalonia.PixelColor.Utils.OpenGl.IsfScene;

namespace Avalonia.PixelColor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        ScalingChanged += MainWindow_ScalingChanged;
        SetScaleFactor();
        ViewModel.ScreenShotControl = OpenGlControl;
    }
    
    private void MainWindow_ScalingChanged(Object? sender, EventArgs e)
    {
        SetScaleFactor();
    }

    private void SetScaleFactor()
    {
        var desktopScaling = RenderScaling;
        SetScaleFactor(desktopScaling);
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

        txtFS.Text = scene.IsfFragmentShaderSource;
        txtVS.Text = scene.IsfVertexShaderSource;
    }

    private void OnApplyClick(Object sender, RoutedEventArgs e)
    {
        var scene = (IsfScene)OpenGlControl.SelectedScene;
        scene.SetUp(txtVS.Text, txtFS.Text);
        ViewModel.UpdateParameters(scene.Parameters);

        EditorPanel.IsVisible = false;
        ViewModel.ShowEditorButtonVisible = true;
    }
}