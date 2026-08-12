using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicBetter.Models;
using MusicBetter.Services;

namespace MusicBetter.ViewModels;

public partial class TrackItemViewModel : ObservableObject
{
    public Track Track { get; }

    public string Title => Track.Title;
    public string Subtitle => Track.Source == TrackSource.Local
        ? $"{Track.Artist} | {Track.Album}"
        : Track.Artist + (Track.Source == TrackSource.YouTube ? " · YouTube" : " · SoundCloud");

    public string ArtworkPath => Track.ArtworkPath ?? "note_placeholder.png";

    [ObservableProperty]
    private bool isFavorite;

    public TrackItemViewModel(Track track)
    {
        Track = track;
        isFavorite = track.IsFavorite;
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        PlayerService.Instance.ToggleFavorite(Track);
        IsFavorite = !IsFavorite;
    }
}
