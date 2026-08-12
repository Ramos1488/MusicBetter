using CommunityToolkit.Maui;
using MusicBetter.ViewModels;
using MusicBetter.Views;

namespace MusicBetter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        // ViewModels
        builder.Services.AddSingleton<MainViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EmbedPlayerPage>();

        return builder.Build();
    }
}