using System.ComponentModel;
using System.Runtime.CompilerServices;
using MusicBetter.Models;
using MusicBetter.Data;

namespace MusicBetter.Services;

public class PlayerService : INotifyPropertyChanged
{
    private static PlayerService? _instance;
    public static PlayerService Instance => _instance ??= new PlayerService();

    private Track? _currentTrack;
    private bool _isPlaying;
    private List<Track> _queue = new();
    private int _currentIndex = -1;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<Track>? RequestExternalPlayback;

    public Track? CurrentTrack
    {
        get => _currentTrack;
        private set
        {
            if (_currentTrack == value) return;
            _currentTrack = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (_isPlaying == value) return;
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    private PlayerService()
    {
        // пустой конструктор — ничего тяжёлого не делаем
    }

    public void SetQueue(IEnumerable<Track> tracks, Track startTrack)
    {
        _queue = tracks.ToList();
        _currentIndex = _queue.FindIndex(t => t.Id == startTrack.Id || t.SourceId == startTrack.SourceId);

        if (_currentIndex < 0)
        {
            _queue.Insert(0, startTrack);
            _currentIndex = 0;
        }

        PlayCurrent();
    }

    public void TogglePlayPause()
    {
        if (CurrentTrack == null) return;

        if (CurrentTrack.Source == TrackSource.Local)
        {
            // Локальное воспроизведение пока заглушка
            IsPlaying = !IsPlaying;
        }
        else
        {
            // YouTube / SoundCloud — открываем внешний плеер
            RequestExternalPlayback?.Invoke(CurrentTrack);
        }
    }

    public void Next()
    {
        if (_queue.Count == 0) return;

        _currentIndex++;
        if (_currentIndex >= _queue.Count)
            _currentIndex = 0;

        PlayCurrent();
    }

    public void Previous()
    {
        if (_queue.Count == 0) return;

        _currentIndex--;
        if (_currentIndex < 0)
            _currentIndex = _queue.Count - 1;

        PlayCurrent();
    }

    public void ToggleFavorite(Track track)
    {
        _ = AppDatabase.Instance.ToggleFavoriteAsync(track);
    }

    private void PlayCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= _queue.Count) return;

        CurrentTrack = _queue[_currentIndex];

        if (CurrentTrack.Source == TrackSource.Local)
        {
            IsPlaying = true;
            // TODO: реальное воспроизведение локального файла
        }
        else
        {
            // Для стриминга сразу открываем встроенный плеер
            RequestExternalPlayback?.Invoke(CurrentTrack);
            IsPlaying = true;
        }

        // Отмечаем, что трек слушали
        _ = AppDatabase.Instance.MarkPlayedAsync(CurrentTrack);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}