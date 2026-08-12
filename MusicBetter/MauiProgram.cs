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

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EmbedPlayerPage>();

        return builder.Build();
    }
}