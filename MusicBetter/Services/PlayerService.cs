using CommunityToolkit.Mvvm.ComponentModel;
using MusicBetter.Data;
using MusicBetter.Models;

namespace MusicBetter.Services;

/// <summary>
/// Центральный сервис плеера. Локальные треки играют через CommunityToolkit MediaElement
/// напрямую с диска (полностью офлайн). Треки из YouTube/SoundCloud открываются через
/// EmbedPlayerPage (WebView с официальным плеером площадки) — там нужен интернет,
/// это ограничение самих сервисов, а не приложения.
/// </summary>
public partial class PlayerService : ObservableObject
{
    private static PlayerService? _instance;
    public static PlayerService Instance => _instance ??= new PlayerService();

    [ObservableProperty]
    private Track? currentTrack;

    [ObservableProperty]
    private bool isPlaying;

    [ObservableProperty]
    private List<Track> queue = new();

    private int _queueIndex = -1;

    public event Action<Track>? RequestExternalPlayback; // для YouTube/SoundCloud -> открыть EmbedPlayerPage

    public void SetQueue(IEnumerable<Track> tracks, Track startWith)
    {
        Queue = tracks.ToList();
        _queueIndex = Queue.FindIndex(t => t.Id == startWith.Id);
        Play(startWith);
    }

    public async void Play(Track track)
    {
        CurrentTrack = track;
        IsPlaying = true;
        await AppDatabase.Instance.MarkPlayedAsync(track);

        if (track.Source != TrackSource.Local)
        {
            RequestExternalPlayback?.Invoke(track);
        }
    }

    public void TogglePlayPause() => IsPlaying = !IsPlaying;

    public void Next()
    {
        if (Queue.Count == 0 || _queueIndex < 0) return;
        _queueIndex = (_queueIndex + 1) % Queue.Count;
        Play(Queue[_queueIndex]);
    }

    public void Previous()
    {
        if (Queue.Count == 0 || _queueIndex < 0) return;
        _queueIndex = (_queueIndex - 1 + Queue.Count) % Queue.Count;
        Play(Queue[_queueIndex]);
    }

    public async void ToggleFavorite(Track track)
    {
        await AppDatabase.Instance.ToggleFavoriteAsync(track);
    }
}
