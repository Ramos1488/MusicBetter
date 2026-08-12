using System.Text.Json;
using MusicBetter.Models;

namespace MusicBetter.Services;

/// <summary>
/// Поиск через официальный YouTube Data API v3.
/// Воспроизведение — только через официальный IFrame Player (см. Views/EmbedPlayerPage),
/// что соответствует условиям использования YouTube. Скачивание видео/аудио НЕ поддерживается
/// и намеренно не реализовано — это нарушало бы ToS YouTube.
///
/// Нужен собственный API-ключ: https://console.cloud.google.com/apis/credentials
/// Впиши его в AppConfig.YouTubeApiKey перед сборкой.
/// </summary>
public class YouTubeSearchService
{
    private readonly HttpClient _http = new();

    public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(AppConfig.YouTubeApiKey))
            throw new InvalidOperationException("Укажи YouTubeApiKey в AppConfig, чтобы включить поиск по YouTube.");

        var url = "https://www.googleapis.com/youtube/v3/search" +
                   $"?part=snippet&type=video&videoCategoryId=10&maxResults={maxResults}" +
                   $"&q={Uri.EscapeDataString(query)}&key={AppConfig.YouTubeApiKey}";

        using var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var results = new List<SearchResult>();
        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
        {
            var videoId = item.GetProperty("id").GetProperty("videoId").GetString() ?? string.Empty;
            var snippet = item.GetProperty("snippet");

            results.Add(new SearchResult
            {
                Source = TrackSource.YouTube,
                SourceId = videoId,
                Title = snippet.GetProperty("title").GetString() ?? "Без названия",
                Artist = snippet.GetProperty("channelTitle").GetString() ?? "",
                ThumbnailUrl = snippet.GetProperty("thumbnails").GetProperty("medium").GetProperty("url").GetString() ?? "",
                EmbedUrl = $"https://www.youtube.com/embed/{videoId}?autoplay=1&playsinline=1"
            });
        }

        return results;
    }
}
