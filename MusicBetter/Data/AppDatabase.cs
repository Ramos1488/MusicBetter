using MusicBetter.Models;
using SQLite;

namespace MusicBetter.Data;

/// <summary>
/// Вся база данных хранится локально на устройстве (файл .db3 в AppDataDirectory).
/// Это значит: избранное, локальные треки и плейлисты работают полностью офлайн,
/// без какого-либо облака и без рекламы/трекеров.
/// </summary>
public class AppDatabase
{
    private readonly SQLiteAsyncConnection _db;
    private static AppDatabase? _instance;

    public static AppDatabase Instance =>
        _instance ??= new AppDatabase(Path.Combine(FileSystem.AppDataDirectory, "musicbetter.db3"));

    private AppDatabase(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
        _db.CreateTableAsync<Track>().Wait();
        _db.CreateTableAsync<Playlist>().Wait();
        _db.CreateTableAsync<PlaylistTrack>().Wait();
    }

    // --- Tracks ---

    public Task<List<Track>> GetAllTracksAsync() =>
        _db.Table<Track>().OrderByDescending(t => t.AddedAt).ToListAsync();

    public Task<List<Track>> GetFavoritesAsync() =>
        _db.Table<Track>().Where(t => t.IsFavorite).ToListAsync();

    public Task<List<Track>> GetRecentAsync(int limit = 30) =>
        _db.Table<Track>()
           .Where(t => t.LastPlayedAt != null)
           .OrderByDescending(t => t.LastPlayedAt)
           .Take(limit)
           .ToListAsync();

    public Task<Track?> FindBySourceAsync(TrackSource source, string sourceId) =>
        _db.Table<Track>()
           .Where(t => t.Source == source && t.SourceId == sourceId)
           .FirstOrDefaultAsync()!;

    public async Task<int> SaveTrackAsync(Track track)
    {
        if (track.Id != 0)
        {
            await _db.UpdateAsync(track);
            return track.Id;
        }

        await _db.InsertAsync(track);
        return track.Id;
    }

    public Task<int> DeleteTrackAsync(Track track) => _db.DeleteAsync(track);

    public async Task ToggleFavoriteAsync(Track track)
    {
        track.IsFavorite = !track.IsFavorite;
        await SaveTrackAsync(track);
    }

    public async Task MarkPlayedAsync(Track track)
    {
        track.LastPlayedAt = DateTime.UtcNow;
        await SaveTrackAsync(track);
    }

    // --- Playlists ---

    public Task<List<Playlist>> GetPlaylistsAsync() =>
        _db.Table<Playlist>().OrderBy(p => p.Name).ToListAsync();

    public async Task<int> CreatePlaylistAsync(string name)
    {
        var playlist = new Playlist { Name = name };
        await _db.InsertAsync(playlist);
        return playlist.Id;
    }

    public async Task AddToPlaylistAsync(int playlistId, int trackId)
    {
        var count = await _db.Table<PlaylistTrack>().Where(pt => pt.PlaylistId == playlistId).CountAsync();
        await _db.InsertAsync(new PlaylistTrack { PlaylistId = playlistId, TrackId = trackId, SortOrder = count });
    }

    public async Task<List<Track>> GetPlaylistTracksAsync(int playlistId)
    {
        var links = await _db.Table<PlaylistTrack>()
            .Where(pt => pt.PlaylistId == playlistId)
            .OrderBy(pt => pt.SortOrder)
            .ToListAsync();

        var tracks = new List<Track>();
        foreach (var link in links)
        {
            var track = await _db.Table<Track>().Where(t => t.Id == link.TrackId).FirstOrDefaultAsync();
            if (track != null) tracks.Add(track);
        }
        return tracks;
    }
}
