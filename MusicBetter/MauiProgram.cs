using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
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
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<EmbedPlayerPage>();

        return builder.Build();
    }
}