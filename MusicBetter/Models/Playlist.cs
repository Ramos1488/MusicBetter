using SQLite;

namespace MusicBetter.Models;

[Table("Playlists")]
public class Playlist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "Новый плейлист";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("PlaylistTracks")]
public class PlaylistTrack
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlaylistId { get; set; }

    [Indexed]
    public int TrackId { get; set; }

    public int SortOrder { get; set; }
}
