using Android.Media;
using Android.Widget;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TuneTaster
{
    class SongsPage : ContentPage
    {

        // --- Start of variables --//
        // Header.
        Picker resultsTypePicker;
        SearchBar searchBar;
        List<string> searchTypes;
        string type = "track";
        ActivityIndicator loadingSpinner;
        StackLayout header;

        // Main.
        TableView songsView;
        StackLayout albumsTopLeftLayout;
        StackLayout albumsTopRightLayout;
        Frame albumFrame;
        Grid albumsGridLayout;
        Xamarin.Forms.ScrollView albumsScrollLayout;
        StackLayout artistsTopLeftLayout;
        StackLayout artistsTopRightLayout;
        Grid artistsGridLayout;
        Frame artistFrame;
        Xamarin.Forms.ScrollView artistsScrollLayout;
        StackLayout main;

        // Footer.
        Xamarin.Forms.Image trackAlbumImage;
        Label trackName;
        Label trackArtist;
        Xamarin.Forms.Image playOrPause;
        StackLayout footerTrackDetailsLayout;
        StackLayout footerTrackDetailsAndImageLayout;
        Grid footerBarLayout;
        StackLayout footer;

        // Music functionality.
        private static SpotifyWebAPI _spotify;
        SearchItem searchItem;
        MediaPlayer player;

        // Extras
        static ClientCredentialsAuth auth;
        Token token;
        int albumIndex = 1;
        int artistIndex = 1;
        Toast previewToast;
        // --- End of variables --//

        public SongsPage()
        {
            // --- Spotify Authentication --- //
            auth = new ClientCredentialsAuth()
            {
                ClientId = "333de38e4a2744fcbe33eae8a39d8bce",
                ClientSecret = "141675ff5e274186b45ef48493a9a5bf",
                Scope = Scope.UserReadPrivate,
            };
            token = auth.DoAuth();
            _spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken,
            };



            // --- Header Content --- //
            searchBar = new SearchBar
            {
                Placeholder = "Search Songs",
                TextColor = Color.White,
                PlaceholderColor = Color.White,
                CancelButtonColor = Color.White,
                SearchCommand = new Command(() => {
                    if (type == "track")
                    {
                        loadingSpinner.IsVisible = true;
                        loadingSpinner.IsRunning = true;
                        albumsScrollLayout.IsVisible = false;
                        artistsScrollLayout.IsVisible = false;
                        artistsTopLeftLayout.Children.Clear();
                        artistsTopRightLayout.Children.Clear();
                        albumsTopLeftLayout.Children.Clear();
                        albumsTopRightLayout.Children.Clear();
                        songsView.Root.Clear();

                        Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Track, 50))
                            .ContinueWith(antecendent =>
                            {
                                foreach(FullTrack track in searchItem.Tracks.Items)
                                {
                                    if (track.Album.Images.Count > 0)
                                    {
                                        UpdateSongList(track.Album.Images[0].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                                    }
                                    else
                                    {
                                        UpdateSongList("Icon.png", track.Name, track.Artists[0].Name, track.PreviewUrl);
                                    }
                                    
                                }
                                loadingSpinner.IsVisible = false;
                                loadingSpinner.IsRunning = false;
                                songsView.IsVisible = true;
                            },
                            TaskScheduler.FromCurrentSynchronizationContext()
                            );
                    }
                    else if (type == "album")
                    {
                        loadingSpinner.IsVisible = true;
                        loadingSpinner.IsRunning = true;
                        artistsScrollLayout.IsVisible = false;
                        songsView.IsVisible = false;
                        albumsTopLeftLayout.Children.Clear();
                        albumsTopRightLayout.Children.Clear();
                        artistsTopLeftLayout.Children.Clear();
                        artistsTopRightLayout.Children.Clear();
                        songsView.Root.Clear();

                        Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Album, 50))
                            .ContinueWith(antecendent =>
                            {
                                foreach (SimpleAlbum album in searchItem.Albums.Items)
                                {
                                    albumIndex++;
                                    if (album.Images.Count > 0)
                                    {
                                        UpdateAlbumList(album, album.Images[0].Url, album.Name, album.Type);
                                    }
                                    else
                                    {
                                        UpdateAlbumList(album, "Icon.png", album.Name, album.Type);
                                    }
                                    
                                }
                                loadingSpinner.IsVisible = false;
                                loadingSpinner.IsRunning = false;
                                albumsScrollLayout.IsVisible = true;
                            },
                            TaskScheduler.FromCurrentSynchronizationContext()
                            );
                    }
                    else if (type == "artist")
                    {
                        loadingSpinner.IsVisible = true;
                        loadingSpinner.IsRunning = true;
                        albumsScrollLayout.IsVisible = false;
                        songsView.IsVisible = false;
                        albumsTopLeftLayout.Children.Clear();
                        albumsTopRightLayout.Children.Clear();
                        artistsTopLeftLayout.Children.Clear();
                        artistsTopRightLayout.Children.Clear();
                        songsView.Root.Clear(); 

                        Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Artist, 50))
                            .ContinueWith(antecendent =>
                            {
                                foreach (FullArtist artist in searchItem.Artists.Items)
                                {
                                    artistIndex++;
                                    if (artist.Images.Count > 0)
                                    {
                                        UpdateArtistList(artist, artist.Images[0].Url, artist.Name);
                                    }
                                    else
                                    {
                                        UpdateArtistList(artist, "Icon.png", artist.Name);
                                    }
                                }
                                    loadingSpinner.IsVisible = false;
                                    loadingSpinner.IsRunning = false;
                                    artistsScrollLayout.IsVisible = true;
                            },
                            TaskScheduler.FromCurrentSynchronizationContext()
                            );
                    }
                })
            };
            searchTypes = new List<string>
            {
                "Tracks", "Albums", "Artists"
            };
            resultsTypePicker = new Picker
            {
                Title = " Results Type",
                TextColor = Color.White,
                ItemsSource = searchTypes
            };
            resultsTypePicker.SelectedIndexChanged += (sender, args) =>
            {
                if (resultsTypePicker.SelectedIndex == 0)
                {
                    type = "track";
                }
                else if (resultsTypePicker.SelectedIndex == 1)
                {
                    type = "album";
                }
                else if (resultsTypePicker.SelectedIndex == 2)
                {
                    type = "artist";
                }
            };
            loadingSpinner = new ActivityIndicator()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                IsVisible = false,
                IsRunning = false,
                Color = Color.FromHex("FFAB40"),
            };



            // --- Main Content --- //
            songsView = new TableView
            {
                RowHeight = 70,
                Intent = TableIntent.Data,
                IsVisible = false,
            };

            albumsTopLeftLayout = new StackLayout { };
            albumsTopRightLayout = new StackLayout { };
            albumsGridLayout = new Grid
            {
                BackgroundColor = Color.FromHex("f2f2f2"),
                Padding = new Thickness(7, 12, 7, 0),
            };
            albumsGridLayout.Children.Add(albumsTopLeftLayout, 0, 0); // Top left
            albumsGridLayout.Children.Add(albumsTopRightLayout, 1, 0); // Top right
            albumsScrollLayout = new Xamarin.Forms.ScrollView
            {
                Content = albumsGridLayout,
                IsVisible = false
            };

            artistsTopLeftLayout = new StackLayout { };
            artistsTopRightLayout = new StackLayout { };
            artistsGridLayout = new Grid
            {
                BackgroundColor = Color.FromHex("f2f2f2"),
                Padding = new Thickness(7, 12, 7, 0),
            };
            artistsGridLayout.Children.Add(artistsTopLeftLayout, 0, 0); // Top left
            artistsGridLayout.Children.Add(artistsTopRightLayout, 1, 0); // Top right
            artistsScrollLayout = new Xamarin.Forms.ScrollView
            {
                Content = artistsGridLayout,
                IsVisible = false
            };




            // --- Footer Content --- //
            trackAlbumImage = new Xamarin.Forms.Image{};
            trackName = new Label{TextColor = Color.Black };
            trackArtist = new Label{TextColor = Color.White};
            playOrPause = new Xamarin.Forms.Image{Source = "pause.png" };

            footerTrackDetailsLayout = new StackLayout
            {
                Padding = new Thickness(4, 4, 0, 0),
                Children = {
                    trackName,
                    trackArtist
                }
            };
            footerTrackDetailsAndImageLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = {
                    trackAlbumImage,
                    footerTrackDetailsLayout
                }
            };
            footerBarLayout = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("4FC3F7"),
                Padding = new Thickness(4, 6, 4, 6),
            };
            footerBarLayout.Children.Add(footerTrackDetailsAndImageLayout, 0, 0);
            footerBarLayout.Children.Add(new BoxView {}, 1, 0);
            footerBarLayout.Children.Add(new BoxView {}, 2, 0);
            footerBarLayout.Children.Add(playOrPause, 3, 0);
            Grid.SetColumnSpan(footerTrackDetailsAndImageLayout, 3);

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






            header = new StackLayout
            {
                BackgroundColor = Color.FromHex("03A9F4"),
                Padding = new Thickness(0, 5, 0, 5),
                Children = {
                    resultsTypePicker,
                    searchBar,
                }
            };
            main = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    loadingSpinner,
                    songsView,
                    albumsScrollLayout,
                    artistsScrollLayout,
                }
            };
            footer = new StackLayout
            {
                IsVisible = false,
                HeightRequest = 70,
                Children = {
                    footerBarLayout
                }
            };

            // --- Build Page --- //
            Content = new StackLayout
            {
                Spacing = 0,
                Children = {
                    header,
                    main,
                    footer
                }
            };
        }



        /// <summary>
        /// When the user types something into the keyboard and presses search this function is called. The Spotify API is called 
        /// and only told to display tracks. For every song that is returned from Spotify(50), a new ImageCell will be created
        /// displaying the track's name, album name, and album cover image.
        /// </summary>
        public void UpdateSongList(ImageSource trackImageSource, string trackName, string trackArtist, string trackPreviewUrl)
        {
            songsView.Root.Add(
            new TableSection
            {
                new ImageCell
                {
                    // Some differences with loading images in initial release.
                    ImageSource = trackImageSource,
                    Text = trackName,
                    Detail = trackArtist,
                    TextColor = Color.FromHex("4FC3F7"),
                    Command = new Command(() =>
                    {
                        UpdateFooterSong(trackImageSource, trackName, trackArtist);
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
                        if (trackPreviewUrl != null)
                        {
                            player.SetDataSource (trackPreviewUrl);
                            player.Prepare();
                            player.Start();
                        }
                        else
                        {
                            if (previewToast != null)
                            {
                                previewToast.Cancel();
                            }
                            previewToast = Toast.MakeText(Android.App.Application.Context, "No preview avaiable!", ToastLength.Short);
                            previewToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                            previewToast.Show();               
                        }
                        
                    })
                }
            }
            );
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="album"></param>
        /// <param name="albumImageSource"></param>
        /// <param name="albumName"></param>
        /// <param name="albumType"></param>
        public void UpdateAlbumList(SimpleAlbum album, ImageSource albumImageSource, string albumName, string albumType)
        {
            albumFrame = new Frame
            {
                HeightRequest = 250,
                Padding = 0,
                GestureRecognizers = {
                    new TapGestureRecognizer {
                        Command = new Command(() => {
                            albumsScrollLayout.IsVisible = false;
                            artistsScrollLayout.IsVisible = false;
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
                Content = new StackLayout
                {
                    Children = {
                        new Xamarin.Forms.Image
                        {
                            Source = albumImageSource,
                        },
                        new Label
                        {
                            Text = albumName,
                            TextColor = Color.FromHex("FFAB40"),
                            Margin = new Thickness (5, 0, 0, 0),
                            HeightRequest = 18
                        },
                        new Label
                        {
                            Text = albumType,
                            FontSize = 11,
                            Margin = new Thickness (5, 0, 0, 0),
                            HeightRequest = 18
                        }
                    }
                }
            };
            if (albumIndex % 2 == 0)
            {
                albumsTopLeftLayout.Children.Add(albumFrame);
            }
            else
            {
                albumsTopRightLayout.Children.Add(albumFrame);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="artistImageSource"></param>
        /// <param name="artistName"></param>
        public void UpdateArtistList(FullArtist artist, ImageSource artistImageSource, string artistName)
        {
            artistFrame = new Frame
            {
                HeightRequest = 250,
                Padding = 0,
                GestureRecognizers = {
                    new TapGestureRecognizer {
                        Command = new Command(() => {
                            artistsScrollLayout.IsVisible = false;
                            songsView.IsVisible = false;
                            albumsScrollLayout.IsVisible = true;
                            albumsTopLeftLayout.Children.Clear();
                            albumsTopRightLayout.Children.Clear();
                            footerBarLayout.HeightRequest = 70;

                            albumIndex = 1;
                            Paging<SimpleAlbum> albums = _spotify.GetArtistsAlbums(artist.Id);
                            foreach (var album in albums.Items)
                            {
                                albumIndex++;
                                UpdateAlbumList(album, album.Images[0].Url, album.Name, album.Type);
                            }
                        })
                    },
                },
                Content = new StackLayout
                {
                    Children = {
                        new Xamarin.Forms.Image
                        {
                            Source = artistImageSource,
                        },
                        new Label
                        {
                            Text = artistName,
                            TextColor = Color.FromHex("4DB6AC"),
                            Margin = new Thickness (5, 0, 0, 0),
                            HeightRequest = 18
                        }
                    }
                }
            };
            if (artistIndex % 2 == 0)
            {
                artistsTopLeftLayout.Children.Add(artistFrame);
            }
            else
            {
                artistsTopRightLayout.Children.Add(artistFrame);
            }
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
            footer.IsVisible = true;
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
                albumsScrollLayout.IsVisible = true;
                songsView.IsVisible = false;
            }

            else if (albumsScrollLayout.IsVisible == true)
            {
                artistsScrollLayout.IsVisible = true;
                albumsScrollLayout.IsVisible = false;
            }
            return true;
        }
    }
}