using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Avalonia.Markup.Xaml.Styling; // Додано
using practiceStudioAssetManager.Infrastructure.Data;
using practiceStudioAssetManager.UI.Views;
using System;

namespace practiceStudioAssetManager;

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        
        var dataGridStyle = new StyleInclude(new Uri("avares://practiceStudioAssetManager/App.axaml"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        };
        Styles.Add(dataGridStyle);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}