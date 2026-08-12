using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicBetter.Data;
using MusicBetter.Models;
using MusicBetter.Services;

namespace MusicBetter.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LibraryService _libraryService = new();
    private readonly YouTubeSearchService _youTubeService = new();

    [ObservableProperty]
    private ObservableCollection<TrackItemViewModel> tracks = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    // 0 = Мои треки (локальные + избранное), 1 = YouTube, 2 = SoundCloud
    [ObservableProperty]
    private int activeTabIndex;

    [ObservableProperty]
    private Track? currentTrack;

    [ObservableProperty]
    private bool isPlaying;

    public MainViewModel()
    {
        PlayerService.Instance.PropertyChanged += (_, __) =>
        {
            CurrentTrack = PlayerService.Instance.CurrentTrack;
            IsPlaying = PlayerService.Instance.IsPlaying;
        };
    }

    [RelayCommand]
    public async Task LoadLibraryAsync()
    {
        var all = await AppDatabase.Instance.GetAllTracksAsync();
        Tracks = new ObservableCollection<TrackItemViewModel>(all.Select(t => new TrackItemViewModel(t)));
    }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        var favs = await AppDatabase.Instance.GetFavoritesAsync();
        Tracks = new ObservableCollection<TrackItemViewModel>(favs.Select(t => new TrackItemViewModel(t)));
    }

    [RelayCommand]
    public async Task ImportFilesAsync()
    {
        await _libraryService.ImportFilesAsync();
        await LoadLibraryAsync();
    }

    [RelayCommand]
    public async Task SearchOnlineAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var results = await _youTubeService.SearchAsync(SearchQuery);
        var mapped = new List<TrackItemViewModel>();

        foreach (var r in results)
        {
            var existing = await AppDatabase.Instance.FindBySourceAsync(TrackSource.YouTube, r.SourceId);
            var track = existing ?? new Track
            {
                Source = TrackSource.YouTube,
                SourceId = r.SourceId,
                Title = r.Title,
                Artist = r.Artist,
                ArtworkPath = r.ThumbnailUrl,
                DurationSeconds = r.DurationSeconds
            };

            if (existing is null)
                await AppDatabase.Instance.SaveTrackAsync(track);

            mapped.Add(new TrackItemViewModel(track));
        }

        Tracks = new ObservableCollection<TrackItemViewModel>(mapped);
    }

    [RelayCommand]
    public void PlayTrack(TrackItemViewModel item)
    {
        PlayerService.Instance.SetQueue(Tracks.Select(t => t.Track), item.Track);
    }

    [RelayCommand]
    public void TogglePlayPause() => PlayerService.Instance.TogglePlayPause();

    [RelayCommand]
    public void NextTrack() => PlayerService.Instance.Next();

    [RelayCommand]
    public void PreviousTrack() => PlayerService.Instance.Previous();
}
