using Android.Media;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace TuneTaster
{
    class AlbumsPage : ContentPage
    {
        // --- Start of variables --- //
        // Header.
        SearchBar searchBar;
        StackLayout searchBarLayout;

        // Main
        StackLayout albumTopLeftLayout;
        StackLayout albumTopRightLayout;
        Grid albumsLayout;
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
        ActivityIndicator indicator;
        // --- End of variables --- //

        public AlbumsPage()
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
            Title = "Albums";

            // Create the search bar section.
            searchBar = new SearchBar
            {
                Placeholder = "Search Albums",
                TextColor = Color.White,
                PlaceholderColor = Color.White,
                CancelButtonColor = Color.White,
                SearchCommand = new Command(() => {UpdateAlbumList();})
            };
            searchBarLayout = new StackLayout
            {
                Children = { searchBar },
                BackgroundColor = Color.FromHex("009688"),
                Padding = new Thickness(0, 5, 0, 5)
            };

            // Create the albums section
            albumTopLeftLayout = new StackLayout{};
            albumTopRightLayout = new StackLayout{};
            albumsLayout = new Grid
            {
                BackgroundColor = Color.FromHex("f2f2f2"),
                Padding = new Thickness(7, 12, 7, 0),
                IsEnabled = false,
                IsVisible = false,
            };
            albumsLayout.Children.Add(albumTopLeftLayout, 0, 0); // Top left
            albumsLayout.Children.Add(albumTopRightLayout, 1, 0); // Top right

            // Create the songs table view.
            songsView = new TableView
            {
                IsEnabled = false,
                IsVisible = false,
                RowHeight = 70,
                Intent = TableIntent.Data,
            };

            // Create all necessary views for the bottom footer.
            trackAlbumImage = new Xamarin.Forms.Image {};
            trackName = new Label { TextColor = Color.White };
            trackArtist = new Label { TextColor = Color.White };
            playOrPause = new Xamarin.Forms.Image { Source = "pause.png" };

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
                IsEnabled = false,
                IsVisible = false,
            };
            footerBarLayout.Children.Add(footerTrackDetailsAndImageLayout, 0, 0);
            footerBarLayout.Children.Add(new BoxView { }, 1, 0);
            footerBarLayout.Children.Add(new BoxView { }, 2, 0);
            footerBarLayout.Children.Add(playOrPause, 3, 0);
            Grid.SetColumnSpan(footerTrackDetailsAndImageLayout, 3);

            // Handle when a user taps on the play or pause image.
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) =>
            {
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


            indicator = new ActivityIndicator()
            {
                IsVisible = false,
                IsEnabled = false,
                IsRunning = false,
                Color = Color.Blue,
            };
            

            // Build the page.
            Content = new StackLayout
            {
                Spacing = 0,
                Children = {
                    searchBarLayout,
                    indicator,
                    new ScrollView
                    {
                        Content = albumsLayout,
                    },
                    songsView,
                    footerBarLayout
                }
            };
        }


        /// <summary>
        /// When the user types something into the keyboard and presses search this function is called. The Spotify API is called 
        /// and only told to display tracks. For every song (50) that is returned from Spotify, a new ImageCell will be created
        /// displaying the track's name, album name, and album cover image.
        /// </summary>
        public void UpdateAlbumList()
        {
            indicator.IsEnabled = true;
            indicator.IsVisible = true;
            indicator.IsRunning = true;

            Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Album, 50))
                .ContinueWith(antecendent => PopulateAlbumList(antecendent.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchItem"> What is returned from Spotify based on what the user typed in the search bar.</param>
        public void PopulateAlbumList(SearchItem searchItem)
        {
            // Clear out what was previously filled in the table before we load the new results.
            albumsLayout.IsEnabled = true;
            albumsLayout.IsVisible = true;
            albumTopLeftLayout.IsEnabled = true;
            albumTopLeftLayout.IsVisible = true;
            albumTopRightLayout.IsEnabled = true;
            albumTopRightLayout.IsVisible = true;
            songsView.IsVisible = false;
            albumTopLeftLayout.Children.Clear();
            albumTopRightLayout.Children.Clear();
            songsView.Root.Clear();

            int index = 1; // The index that will determine whether or not an album goes to the right or left.

            // For each song, we want to create a new TableSection (This will display a list of the 20 first songs from Spotify API)
            foreach (var album in searchItem.Albums.Items)
            {
                index++;

                if (index % 2 == 0) // If the index has no remainder (even number), place the album to the left
                {
                    albumTopLeftLayout.Children.Add(
                        new Frame
                        {
                            GestureRecognizers = {
                                new TapGestureRecognizer {
                                    Command = new Command(() => {
                                        albumsLayout.IsEnabled = false;
                                        albumsLayout.IsVisible = false;
                                        albumTopLeftLayout.IsEnabled = false;
                                        albumTopLeftLayout.IsVisible = false;
                                        albumTopRightLayout.IsEnabled = false;
                                        albumTopRightLayout.IsVisible = false;
                                        songsView.IsEnabled = true;
                                        songsView.IsVisible = true;
                                        songsView.Root.Clear();
                                        // Grab what the user typed in the search bar, restrict the API to only search tracks, and store the value as a variable.
                                        Paging<SimpleTrack> tracks = _spotify.GetAlbumTracks(album.Id);

                                        // For each song, we want to create a new TableSection (This will display a list of the 20 first songs from Spotify API)
                                        foreach (var track in tracks.Items)
                                        {
                                            UpdateSongList(album.Images[0].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                                        }
                                    })
                                },
                            },
                            HeightRequest = 235,
                            Padding = 0,
                            Content = new StackLayout
                            {
                                Children = {
                                    new Xamarin.Forms.Image
                                    {
                                        Source = album.Images[0].Url,
                                    },
                                    new Label
                                    {
                                        Text = album.Name,
                                        TextColor = Color.FromHex("4DB6AC"),
                                        Margin = new Thickness (5, 0, 0, 0),
                                        HeightRequest = 18,
                                    },
                                    new Label
                                    {
                                        Text = "Blah",
                                        FontSize = 11,
                                        Margin = new Thickness (5, 0, 0, 0),
                                        HeightRequest = 18
                                    }
                                }
                            }
                        });
                }

                else // If the index is an odd number, place the album to the right.
                {
                    albumTopRightLayout.Children.Add(
                        new Frame
                        {
                            GestureRecognizers = {
                                new TapGestureRecognizer {
                                    Command = new Command(() => {
                                        albumsLayout.IsEnabled = false;
                                        albumsLayout.IsVisible = false;
                                        albumTopLeftLayout.IsEnabled = false;
                                        albumTopLeftLayout.IsVisible = false;
                                        albumTopRightLayout.IsEnabled = false;
                                        albumTopRightLayout.IsVisible = false;
                                        songsView.IsEnabled = true;
                                        songsView.IsVisible = true;
                                        songsView.Root.Clear();
                                        // Grab what the user typed in the search bar, restrict the API to only search tracks, and store the value as a variable.
                                        Paging<SimpleTrack> tracks = _spotify.GetAlbumTracks(album.Id);

                                        // For each song, we want to create a new TableSection (This will display a list of the 20 first songs from Spotify API)
                                        foreach (var track in tracks.Items)
                                        {
                                            UpdateSongList(album.Images[0].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                                        }
                                    })
                                },
                            },
                            HeightRequest = 235,
                            Padding = 0,
                            Content = new StackLayout
                            {
                                Children = {
                                    new Xamarin.Forms.Image
                                    {
                                        Source = album.Images[0].Url,
                                    },
                                    new Label
                                    {
                                        Text = album.Name,
                                        TextColor = Color.FromHex("4DB6AC"),
                                        Margin = new Thickness (5, 0, 0, 0),
                                        HeightRequest = 18
                                    },
                                    new Label
                                    {
                                        Text = "Blah",
                                        FontSize = 11,
                                        Margin = new Thickness (5, 0, 0, 0),
                                        HeightRequest = 18
                                    }
                                }
                            }
                        });
                }
            }

            indicator.IsEnabled = false;
            indicator.IsVisible = false;
            indicator.IsRunning = false;
        }




        /// <summary>
        /// 
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
            footerBarLayout.IsEnabled = true;
            footerBarLayout.IsVisible = true;
            trackAlbumImage.Source = songAlbumImage;
            trackName.Text = songName;
            trackArtist.Text = songArtistName;
        }



        /// <summary>
        /// If the user is has selected an album and wants to return, this will override the default Android back button behaviour to show the 
        /// list of albums.
        /// </summary>
        /// <returns>Prevents the app from closing.</returns>
        protected override bool OnBackButtonPressed()
        {
            if (songsView.IsVisible == true)
            {
                albumsLayout.IsEnabled = true;
                albumsLayout.IsVisible = true;
                albumTopLeftLayout.IsEnabled = true;
                albumTopLeftLayout.IsVisible = true;
                albumTopRightLayout.IsEnabled = true;
                albumTopRightLayout.IsVisible = true;
                songsView.IsVisible = false;
            }
            return true;
        }
    }
}