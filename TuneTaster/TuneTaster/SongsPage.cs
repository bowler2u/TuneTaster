using Android.Media;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using Xamarin.Forms;

namespace TuneTaster
{
    class SongsPage : ContentPage
    {
        // --- Start of variables --//
        // Header.
        SearchBar searchBar;
        StackLayout searchBarLayout;

        // Main
        TableView songsView;

        // Footer.
        Xamarin.Forms.Image trackAlbumImage;
        Label trackName;
        Label trackArtist;
        Xamarin.Forms.Image playOrPause;
        StackLayout footerTrackDetailsLayout;
        StackLayout footerTrackDetailsAndImageLayout;
        Grid footerBarLayout;

        // Music functionality.
        private static SpotifyWebAPI _spotify;
        static ClientCredentialsAuth auth;
        Token token;
        SearchItem searchItem;
        MediaPlayer player;
        // --- End of variables --//

        public SongsPage()
        {
            // Authenticate Spotify
            auth = new ClientCredentialsAuth()
            {
                ClientId = "333de38e4a2744fcbe33eae8a39d8bce",
                ClientSecret = "141675ff5e274186b45ef48493a9a5bf",
                Scope = Scope.UserReadPrivate,
            };
            //With this token object, I can now make calls
            token = auth.DoAuth();
            _spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken,
            };

            // Title of the individual tabbed page.
            Title = "Songs";

            // Create the searchbar section.
            searchBar = new SearchBar
            {
                Placeholder = "Search Songs",
                TextColor = Color.White,
                PlaceholderColor = Color.White,
                CancelButtonColor = Color.White,
                SearchCommand = new Command(() => {
                    // Grab what the user typed in the search bar, restrict the API to only search tracks, and store the value as a variable.
                    searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Track, 50);

                    // Clear out what was previously filled in the table before we load the new results.
                    songsView.IsVisible = true;
                    songsView.Root.Clear();

                    // For each song, we want to create a new TableSection (This will display a list of the 20 first songs from Spotify API)
                    foreach (var song in searchItem.Tracks.Items)
                    {
                        UpdateSongList(song.Album.Images[1].Url, song.Name, song.Artists[0].Name, song.PreviewUrl);
                    }
                })
            };
            searchBarLayout = new StackLayout
            {
                Children = {searchBar},
                BackgroundColor = Color.FromHex("009688"),
                Padding = new Thickness(0, 5, 0, 5)
            };


            // Create the songs section.
            songsView = new TableView
            {
                RowHeight = 70,
                Intent = TableIntent.Data,
                IsVisible = false
            };


            // Create all necessary views for the bottom footer.
            trackAlbumImage = new Xamarin.Forms.Image{};
            trackName = new Label{TextColor = Color.White};
            trackArtist = new Label{TextColor = Color.White};
            playOrPause = new Xamarin.Forms.Image{Source = "pause.png" };

            // Create the footer track name and artist layout
            footerTrackDetailsLayout = new StackLayout
            {
                Padding = new Thickness(4, 4, 0, 0),
                Children = {
                    trackName,
                    trackArtist
                }
            };

            // Create the footer track album, name, and artist layout.
            footerTrackDetailsAndImageLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = {
                    trackAlbumImage,
                    footerTrackDetailsLayout
                }
            };

            // Create the footer bar layout
            footerBarLayout = new Grid
            {
                BackgroundColor = Color.FromHex("4DB6AC"),
                Padding = new Thickness(4, 6, 4, 6),
                HeightRequest = 70,
                IsVisible = false,
            };
            footerBarLayout.Children.Add(footerTrackDetailsAndImageLayout, 0, 0);
            footerBarLayout.Children.Add(new BoxView {}, 1, 0);
            footerBarLayout.Children.Add(new BoxView {}, 2, 0);
            footerBarLayout.Children.Add(playOrPause, 3, 0);
            Grid.SetColumnSpan(footerTrackDetailsAndImageLayout, 3);

            // Handle when a user taps on the play or pause image.
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                if (player.IsPlaying)
                {
                    player.Pause();
                    playOrPause.Source = "play.png";
                }
                else
                {
                    player.Start();
                    playOrPause.Source = "pause.png";
                }
            };
            playOrPause.GestureRecognizers.Add(tapGestureRecognizer);


            // Build the page.
            Content = new StackLayout
            {
                Spacing = 0,
                Children = {
                    searchBarLayout,
                    songsView,
                    footerBarLayout
                }
            };
        }


        /// <summary>
        /// When the user types something into the keyboard and presses search this function is called. The Spotify API is called 
        /// and only told to display tracks. For every song that is returned from Spotify(50), a new ImageCell will be created
        /// displaying the track's name, album name, and album cover image.
        /// </summary>
        public void UpdateSongList(ImageSource trackAlbumImage, string trackName, string trackArtist, string trackPreviewUrl)
        {
            songsView.Root.Add(
            new TableSection
            {
                new ImageCell
                {
                    // Some differences with loading images in initial release.
                    ImageSource = trackAlbumImage,
                    Text = trackName,
                    Detail = trackArtist,
                    TextColor = Color.FromHex("4DB6AC"),
                    Command = new Command(() =>
                    {
                        UpdateFooterSong(trackAlbumImage, trackName, trackArtist);
                        playOrPause.Source = "pause.png";
                        if (player == null)
                        {
                            player = new MediaPlayer();
                            player.SetAudioStreamType (Stream.Music);
                        }
                        else if (player != null)
                        {
                            player.Stop();
                            player.Reset();
                        }
                        player.SetDataSource (trackPreviewUrl);
                        player.Prepare();
                        player.Start();
                    })
                }
            }
            );
        }


        /// <summary>
        /// Whenever a song is taped from the list, this function will get called to display the current playing track the user
        /// just selected at the bottom of the screen.
        /// </summary>
        /// <param name="songName">The name of the track.</param>
        /// <param name="songAlbumName">The name of the track's album.</param>
        /// <param name="songAlbumImage">The cover image of the album.</param>
        public void UpdateFooterSong(ImageSource songAlbumImage, string songName, string songArtistName)
        {
            footerBarLayout.IsVisible = true;
            trackAlbumImage.Source = songAlbumImage;
            trackName.Text = songName;
            trackArtist.Text = songArtistName;
        }
    }
}