using Avaloina.PixelColor.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using pt.CommandExecutor.Common;
using System;

namespace Avaloina.PixelColor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;
        }

        private MainWindowViewModel ViewModel { get; }

        private void OnMakeScreenshotClick(Object sender, RoutedEventArgs e)
        {
            var executor = new CommandExecutor();
            executor.Execute(ViewModel.MakeScreenShotCommand);
        }
    }
}