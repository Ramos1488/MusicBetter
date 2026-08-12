using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicBetter.Data;
using MusicBetter.Models;
using MusicBetter.Services;

namespace MusicBetter.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private LibraryService? _libraryService;
    private YouTubeSearchService? _youTubeService;

    private LibraryService LibraryService => _libraryService ??= new LibraryService();
    private YouTubeSearchService YouTubeService => _youTubeService ??= new YouTubeSearchService();

    [ObservableProperty]
    private ObservableCollection<TrackItemViewModel> tracks = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private int activeTabIndex;

    [ObservableProperty]
    private Track? currentTrack;

    [ObservableProperty]
    private bool isPlaying;

    public MainViewModel()
    {
        try
        {
            PlayerService.Instance.PropertyChanged += (_, __) =>
            {
                CurrentTrack = PlayerService.Instance.CurrentTrack;
                IsPlaying = PlayerService.Instance.IsPlaying;
            };
        }
        catch
        {
            // игнорируем, чтобы не крашить запуск
        }
    }

    [RelayCommand]
    public async Task LoadLibraryAsync()
    {
        try
        {
            var all = await AppDatabase.Instance.GetAllTracksAsync();
            Tracks = new ObservableCollection<TrackItemViewModel>(
                all.Select(t => new TrackItemViewModel(t)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadLibrary error: {ex}");
        }
    }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        try
        {
            var favs = await AppDatabase.Instance.GetFavoritesAsync();
            Tracks = new ObservableCollection<TrackItemViewModel>(
                favs.Select(t => new TrackItemViewModel(t)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadFavorites error: {ex}");
        }
    }

    [RelayCommand]
    public async Task ImportFilesAsync()
    {
        try
        {
            await LibraryService.ImportFilesAsync();
            await LoadLibraryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImportFiles error: {ex}");
        }
    }

    [RelayCommand]
    public async Task SearchOnlineAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        try
        {
            var results = await YouTubeService.SearchAsync(SearchQuery);
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SearchOnline error: {ex}");
        }
    }

    [RelayCommand]
    public void PlayTrack(TrackItemViewModel item)
    {
        try
        {
            PlayerService.Instance.SetQueue(Tracks.Select(t => t.Track), item.Track);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PlayTrack error: {ex}");
        }
    }

    [RelayCommand]
    public void TogglePlayPause()
    {
        try { PlayerService.Instance.TogglePlayPause(); }
        catch { }
    }

    [RelayCommand]
    public void NextTrack()
    {
        try { PlayerService.Instance.Next(); }
        catch { }
    }

    [RelayCommand]
    public void PreviousTrack()
    {
        try { PlayerService.Instance.Previous(); }
        catch { }
    }
}