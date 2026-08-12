using System.Text.Json;
using MusicBetter.Models;

namespace MusicBetter.Services;

/// <summary>
/// Поиск и воспроизведение через официальный SoundCloud Widget API (w.soundcloud.com/player).
/// SoundCloud с 2020 года выдаёт доступ к полноценному API поиска только по заявке
/// (https://developers.soundcloud.com/docs/api/guide) — до одобрения приложения
/// этот сервис работает через resolve публичных ссылок на треки/плейлисты, которые
/// пользователь сам вставляет, либо использует твой одобренный client_id.
/// Скачивание файлов не реализовано — только официальный embed-плеер.
/// </summary>
public class SoundCloudSearchService
{
    private readonly HttpClient _http = new();

    /// <summary>
    /// Резолвит публичный URL трека SoundCloud (soundcloud.com/artist/track) в embed-виджет.
    /// </summary>
    public async Task<SearchResult?> ResolveTrackUrlAsync(string trackUrl)
    {
        if (string.IsNullOrWhiteSpace(AppConfig.SoundCloudClientId))
            throw new InvalidOperationException("Укажи SoundCloudClientId в AppConfig (получить на developers.soundcloud.com).");

        var resolveUrl = $"https://api.soundcloud.com/resolve?url={Uri.EscapeDataString(trackUrl)}&client_id={AppConfig.SoundCloudClientId}";
        using var response = await _http.GetAsync(resolveUrl);
        if (!response.IsSuccessStatusCode) return null;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        var title = root.TryGetProperty("title", out var t) ? t.GetString() ?? "Без названия" : "Без названия";
        var artist = root.TryGetProperty("user", out var u) && u.TryGetProperty("username", out var un) ? un.GetString() ?? "" : "";
        var artwork = root.TryGetProperty("artwork_url", out var a) ? a.GetString() ?? "" : "";
        var duration = root.TryGetProperty("duration", out var d) ? d.GetInt32() / 1000 : 0;

        return new SearchResult
        {
            Source = TrackSource.SoundCloud,
            SourceId = trackUrl,
            Title = title,
            Artist = artist,
            ThumbnailUrl = artwork,
            DurationSeconds = duration,
            EmbedUrl = "https://w.soundcloud.com/player/?url=" + Uri.EscapeDataString(trackUrl) +
                       "&color=%23c2185b&auto_play=true&show_comments=false&visual=false"
        };
    }
}
