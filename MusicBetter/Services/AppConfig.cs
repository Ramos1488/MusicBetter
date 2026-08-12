namespace MusicBetter.Services;

/// <summary>
/// Ключи API нужно получить самостоятельно и вписать сюда (или лучше — прокинуть
/// через переменные окружения/секреты в CI, если выкладываешь код в открытый GitHub-репозиторий,
/// чтобы не светить ключи в истории коммитов).
///
/// YouTube Data API v3:   https://console.cloud.google.com/apis/library/youtube.googleapis.com
/// SoundCloud API:        https://developers.soundcloud.com/docs/api/guide
/// </summary>
public static class AppConfig
{
    public const string YouTubeApiKey = "";       // <-- вставь свой ключ
    public const string SoundCloudClientId = "";  // <-- вставь свой client_id
}
