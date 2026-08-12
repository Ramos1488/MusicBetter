namespace MusicBetter;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Views.EmbedPlayerPage), typeof(Views.EmbedPlayerPage));
    }
}
