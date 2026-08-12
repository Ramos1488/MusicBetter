namespace MusicBetter.Views;

[QueryProperty(nameof(EmbedUrl), "EmbedUrl")]
[QueryProperty(nameof(TrackTitle), "TrackTitle")]
public partial class EmbedPlayerPage : ContentPage
{
    public string EmbedUrl { get; set; } = string.Empty;
    public string TrackTitle { get; set; } = string.Empty;

    public EmbedPlayerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = this;

        if (!string.IsNullOrWhiteSpace(EmbedUrl))
            PlayerWebView.Source = EmbedUrl;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
