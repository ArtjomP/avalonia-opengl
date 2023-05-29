using Avalonia.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;

namespace Avalonia.PixelColor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        var scaleFactor = this.PlatformImpl.DesktopScaling;
        OpenGlControl.ScaleFactor = scaleFactor;
        ViewModel.ScreenShotControl = OpenGlControl;
    }

    private MainWindowViewModel ViewModel { get; }

    private void OnMakeScreenshotClick(Object sender, RoutedEventArgs e)
    {
        var executor = new CommandExecutor();
        executor.Execute(ViewModel.MakeScreenShotCommand);
    }
}