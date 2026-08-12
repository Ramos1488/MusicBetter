using SQLite;

namespace MusicBetter.Models;

/// <summary>
/// Источник трека. Local — файл на устройстве (всегда доступен офлайн).
/// YouTube/SoundCloud — стриминг через официальные встраиваемые плееры (нужен интернет).
/// </summary>
public enum TrackSource
{
    Local = 0,
    YouTube = 1,
    SoundCloud = 2
}

[Table("Tracks")]
public class Track
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Для локальных файлов — абсолютный путь на диске.
    // Для YouTube — VideoId. Для SoundCloud — track URL / permalink.
    [Indexed]
    public string SourceId { get; set; } = string.Empty;

    public TrackSource Source { get; set; } = TrackSource.Local;

    public string Title { get; set; } = "Неизвестный трек";
    public string Artist { get; set; } = "Неизвестный исполнитель";
    public string Album { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }

    // Локальный путь к обложке (кэш) либо удалённый URL превью.
    public string? ArtworkPath { get; set; }

    public bool IsFavorite { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastPlayedAt { get; set; }

    [Ignore]
    public bool IsAvailableOffline => Source == TrackSource.Local;
}
