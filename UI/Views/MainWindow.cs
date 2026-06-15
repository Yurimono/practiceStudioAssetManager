using Avalonia.Controls;
using Avalonia.Layout;
using practiceStudioAssetManager.Core.Constants;

namespace practiceStudioAssetManager.UI.Views;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = AppConstants.AppTitle;
        Width = UIMetrics.MainWindowWidth;
        Height = UIMetrics.MainWindowHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = new TextBlock
        {
            Text = "123",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}