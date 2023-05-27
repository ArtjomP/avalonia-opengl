using Avalonia.PixelColor.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Avalonia.PixelColor.ViewModels;

public sealed class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        var canExecute = this
            .WhenAnyValue(
                o => o.ScreenShotsFolder,
                o => o.ScreenShotControl)
            .Select(o => !String.IsNullOrEmpty(o.Item1) && o.Item2 is not null);
        MakeScreenShotCommand = ReactiveCommand.Create(MakeScreenShot);
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
                var fullname = Path.Combine(folder, filename);
                screenShotControl.MakeScreenShot(fullname);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [Reactive]
    public IScreenShotControl? ScreenShotControl { get; set; }

    [Reactive]
    public String ScreenShotsFolder { get; set; } = String.Empty;

    [Reactive]
    public String Error { get; set; } = String.Empty;
}