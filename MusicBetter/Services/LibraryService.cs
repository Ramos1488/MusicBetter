using MusicBetter.Data;
using MusicBetter.Models;

namespace MusicBetter.Services;

/// **Summary:**

/// Отвечает за импорт файлов с устройства в локальную библиотеку.
/// Ничего не скачивается из интернета — только файлы, которые пользователь
/// сам выбрал через системный FilePicker (его собственные mp3/flac/wav и т.д.).
/// 
public class LibraryService
{
    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".m4a", ".ogg", ".aac" };

    public async Task<List<Track>> ImportFilesAsync()
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/*" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg", ".aac" } },
                { DevicePlatform.macOS, new[] { "public.audio" } },
                { DevicePlatform.Tizen, new[] { "*/*" } }
            });

        var options = new PickOptions
        {
            PickerTitle = "Выберите музыкальные файлы",
            FileTypes = customFileType
        };

        var files = await FilePicker.Default.PickMultipleAsync(options);
        var imported = new List<Track>();
        if (files is null) return imported;

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!SupportedExtensions.Contains(ext)) continue;

            var track = await BuildTrackFromFileAsync(file.FullPath, file.FileName);
            await AppDatabase.Instance.SaveTrackAsync(track);
            imported.Add(track);
        }

        return imported;
    }

    private static async Task<Track> BuildTrackFromFileAsync(string fullPath, string fileName)
    {
        var track = new Track
        {
            Source = TrackSource.Local,
            SourceId = fullPath,
            Title = Path.GetFileNameWithoutExtension(fileName),
            Artist = "Неизвестный исполнитель",
            Album = "Неизвестный альбом"
        };

        try
        {
            using var tagFile = TagLib.File.Create(fullPath);
            if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title)) track.Title = tagFile.Tag.Title;
            if (tagFile.Tag.Performers.Length > 0) track.Artist = string.Join(", ", tagFile.Tag.Performers);
            if (!string.IsNullOrWhiteSpace(tagFile.Tag.Album)) track.Album = tagFile.Tag.Album;
            track.DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds;

            var picture = tagFile.Tag.Pictures.FirstOrDefault();
            if (picture != null)
            {
                var artDir = Path.Combine(FileSystem.AppDataDirectory, "artwork");
                Directory.CreateDirectory(artDir);
                var artPath = Path.Combine(artDir, $"{Guid.NewGuid()}.jpg");
                await File.WriteAllBytesAsync(artPath, picture.Data.Data);
                track.ArtworkPath = artPath;
            }
        }
        catch
        {
            // Файл без корректных тегов — используем имя файла как есть.
        }

        return track;
    }
}