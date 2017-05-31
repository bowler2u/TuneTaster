using SpotifyAPI.Web;
using System;
using Xamarin.Forms;

namespace TuneTaster
{
    class ArtistsPage : ContentPage
    {
        // Initalize variables.
        private static SpotifyWebAPI _spotify;
        Label resultsLabel;
        SearchBar searchBar;

        public ArtistsPage()
        {
            Title = "Artists";

            resultsLabel = new Label
            {
                Text = "Result will appear here.",
                VerticalOptions = LayoutOptions.FillAndExpand,
                FontSize = 25
            };

            searchBar = new SearchBar
            {
                Placeholder = "Enter search term",
                SearchCommand = new Command(() => { resultsLabel.Text = "Result: " + searchBar.Text + " is what you asked for."; })
            };

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
                Children = {
                new Label {
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = "SearchBar",
                    FontSize = 50
                },
                searchBar,
                new ScrollView
                {
                    Content = resultsLabel,
                    VerticalOptions = LayoutOptions.FillAndExpand
                }
                },
                Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5)
            };
        }
    }
}
