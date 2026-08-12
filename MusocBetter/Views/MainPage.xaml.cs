using MusicBetter.ViewModels;
using MusicBetter.Services;

namespace MusicBetter.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        PlayerService.Instance.RequestExternalPlayback += async track =>
        {
            var url = track.Source == Models.TrackSource.YouTube
                ? $"https://www.youtube.com/embed/{track.SourceId}?autoplay=1&playsinline=1"
                : $"https://w.soundcloud.com/player/?url={Uri.EscapeDataString(track.SourceId)}&auto_play=true";

            await Shell.Current.GoToAsync(nameof(EmbedPlayerPage), new Dictionary<string, object>
            {
                ["EmbedUrl"] = url,
                ["TrackTitle"] = track.Title
            });
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadLibraryCommand.ExecuteAsync(null);
    }
}
