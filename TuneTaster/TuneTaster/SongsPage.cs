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
        // --- Spotify Authentication --- //
        private static ClientCredentialsAuth auth = new ClientCredentialsAuth
        {
            ClientId = "333de38e4a2744fcbe33eae8a39d8bce",
            ClientSecret = "141675ff5e274186b45ef48493a9a5bf",
            Scope = Scope.UserReadPrivate,
        };
        private static Token token = auth.DoAuth();
        private static SpotifyWebAPI _spotify = new SpotifyWebAPI
        {
            TokenType = token.TokenType,
            AccessToken = token.AccessToken,
        };

        // --- Variables --//
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
        SearchItem searchItem;
        MediaPlayer player;

        // Extras
        int albumIndex = 1;
        int artistIndex = 1;
        Toast noPreviewToast;
        Toast noresultsToast;
        TapGestureRecognizer tapGestureRecognizer;


        public SongsPage()
        {
            // --- Header Content --- //
            // Search Bar.
            searchBar = new SearchBar
            {
                Placeholder = "Search Songs",
                TextColor = Color.White,
                PlaceholderColor = Color.White,
                CancelButtonColor = Color.White,
                SearchCommand = new Command(() => {
                    if (type == "track")
                    {
                        SearchTracks();
                    }
                    else if (type == "album")
                    {
                        SearchAlbums();
                    }
                    else if (type == "artist")
                    {
                        SearchArtists();
                    }
                })
            };

            // Picker
            searchTypes = new List<string>{"Tracks", "Albums", "Artists" };
            resultsTypePicker = new Picker
            {
                Title = " Results Type",
                TextColor = Color.White,
                ItemsSource = searchTypes,
                SelectedItem = searchTypes[0],
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

            // Header.
            header = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromHex("3F51B5"),
                Padding = new Thickness(0, 5, 0, 5),
                Children = {
                    searchBar,
                    resultsTypePicker,
                    
                }
            };




            // --- Main Content --- //
            // Loading Spinner.
            loadingSpinner = new ActivityIndicator()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                IsVisible = false,
                IsRunning = false,
                Color = Color.FromHex("FF5252"),
            };

            // Songs.
            songsView = new TableView
            {
                RowHeight = 70,
                Intent = TableIntent.Data,
                IsVisible = false,
            };

            // Albums.
            albumsTopLeftLayout = new StackLayout { };
            albumsTopRightLayout = new StackLayout { };
            albumsGridLayout = new Grid{ };
            albumsGridLayout.Children.Add(albumsTopLeftLayout, 0, 0); // Top left
            albumsGridLayout.Children.Add(albumsTopRightLayout, 1, 0); // Top right
            albumsScrollLayout = new Xamarin.Forms.ScrollView
            {
                IsVisible = false,
                Padding = new Thickness(7, 12, 7, 12),
                Content = albumsGridLayout,
            };

            // Artists.
            artistsTopLeftLayout = new StackLayout { };
            artistsTopRightLayout = new StackLayout { };
            artistsGridLayout = new Grid{ };
            artistsGridLayout.Children.Add(artistsTopLeftLayout, 0, 0); // Top left
            artistsGridLayout.Children.Add(artistsTopRightLayout, 1, 0); // Top right
            artistsScrollLayout = new Xamarin.Forms.ScrollView
            {
                IsVisible = false,          
                Padding = new Thickness(7, 12, 7, 0),
                Content = artistsGridLayout,
            };

            // Main.
            main = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("f2f2f2"),
                Children = {
                    loadingSpinner,
                    songsView,
                    albumsScrollLayout,
                    artistsScrollLayout,
                }
            };


            // --- Footer Content --- //
            trackAlbumImage = new Xamarin.Forms.Image{};
            trackName = new Label{TextColor = Color.White };
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
            footerBarLayout = new Grid{ };
            footerBarLayout.Children.Add(footerTrackDetailsAndImageLayout, 0, 0);
            footerBarLayout.Children.Add(new BoxView {}, 1, 0);
            footerBarLayout.Children.Add(new BoxView {}, 2, 0);
            footerBarLayout.Children.Add(playOrPause, 3, 0);
            Grid.SetColumnSpan(footerTrackDetailsAndImageLayout, 3);

            // Footer.
            footer = new StackLayout
            {
                VerticalOptions = LayoutOptions.End,
                HeightRequest = 70,
                IsVisible = false,            
                BackgroundColor = Color.FromHex("5C6BC0"),
                Padding = new Thickness(4, 6, 4, 6),
                Children = {
                    footerBarLayout
                }
            };


            // --- Non Content actions. --- //
            tapGestureRecognizer = new TapGestureRecognizer();
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

            SizeChanged += (sender, e) =>
            {
                if (this.Width > this.Height)
                {
                    footer.HeightRequest = 110;
                }
                else if (this.Width < this.Height)
                {
                    footer.HeightRequest = 70;
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
        /// 
        /// </summary>
        public void SearchTracks()
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
                    if (searchItem.Tracks.Total == 0)
                    {
                        if (noresultsToast != null)
                        {
                            noresultsToast.Cancel();
                        }
                        noresultsToast = Toast.MakeText(Android.App.Application.Context, "No results!", ToastLength.Long);
                        noresultsToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                        noresultsToast.Show();
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                    }
                    else
                    {
                        foreach (FullTrack track in searchItem.Tracks.Items)
                        {
                            if (track.Album.Images.Count > 0)
                            {
                                UpdateSongList(track.Album.Images[0].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                            }
                            else
                            {
                                UpdateSongList("missingTrack.png", track.Name, track.Artists[0].Name, track.PreviewUrl);
                            }
                        }
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                        songsView.IsVisible = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
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
                    TextColor = Color.FromHex("FF5252"),
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
                            if (noPreviewToast != null)
                            {
                                noPreviewToast.Cancel();
                            }
                            noPreviewToast = Toast.MakeText(Android.App.Application.Context, "No preview avaiable!", ToastLength.Short);
                            noPreviewToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                            noPreviewToast.Show();               
                        }
                        
                    })
                }
            }
            );
        }






        public void SearchAlbums()
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
                    if (searchItem.Albums.Total == 0)
                    {
                        if (noresultsToast != null)
                        {
                            noresultsToast.Cancel();
                        }
                        noresultsToast = Toast.MakeText(Android.App.Application.Context, "No results!", ToastLength.Long);
                        noresultsToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                        noresultsToast.Show();
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                    }
                    else
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
                                UpdateAlbumList(album, "missingAlbum.png", album.Name, album.Type);
                            }
                        }
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                        albumsScrollLayout.IsVisible = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
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






        public void SearchArtists()
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
                    if (searchItem.Artists.Total == 0)
                    {
                        if (noresultsToast != null)
                        {
                            noresultsToast.Cancel();
                        }
                        noresultsToast = Toast.MakeText(Android.App.Application.Context, "No results!", ToastLength.Long);
                        noresultsToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                        noresultsToast.Show();
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                    }
                    else
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
                                UpdateArtistList(artist, "missingArtist.png", artist.Name);
                            }
                        }
                        loadingSpinner.IsVisible = false;
                        loadingSpinner.IsRunning = false;
                        artistsScrollLayout.IsVisible = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
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