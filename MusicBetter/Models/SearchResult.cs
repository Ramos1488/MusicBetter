namespace MusicBetter.Models;

/// <summary>
/// Результат поиска во внешнем источнике (YouTube / SoundCloud).
/// Используется только для отображения и запуска официального встроенного плеера —
/// само аудио никогда не скачивается и не сохраняется в обход площадки.
/// </summary>
public class SearchResult
{
    public string SourceId { get; set; } = string.Empty;
    public TrackSource Source { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }

    /// <summary>URL, который загружается в WebView с официальным embed-плеером площадки.</summary>
    public string EmbedUrl { get; set; } = string.Empty;
}
