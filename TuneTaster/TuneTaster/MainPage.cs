using Android.Media;
using Android.Widget;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TuneTaster
{
    class MainPage : ContentPage
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
        ActivityIndicator searchLoadingSpinner;
        ActivityIndicator trackLoadingSpinner;
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
        Paging<SimpleTrack> albumsTracks;
        Paging<SimpleAlbum> artistsAlbums;
        protected MediaPlayer player;

        // Extras
        bool shouldUserGoBack;
        int albumIndex = 1;
        int artistIndex = 1;
        Toast noPreviewToast;
        Toast noresultsToast;
        TapGestureRecognizer tapGestureRecognizer;


        public MainPage()
        {
            // --- Header Content --- //
            // Search Bar.
            searchBar = new SearchBar
            {
                WidthRequest = 295,
                Placeholder = "Search...",
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
            searchTypes = new List<string> { "Tracks", "Albums", "Artists" };
            resultsTypePicker = new Picker
            {
                Title = " Results Type",
                TextColor = Color.White,
                ItemsSource = searchTypes,
                SelectedItem = searchTypes[0]
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
                VerticalOptions = LayoutOptions.Start,
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Color.FromHex("3F51B5"),
                Padding = new Thickness(0, 5, 0, 5),
                Children = {
                    searchBar,
                    resultsTypePicker,
                }
            };




            // --- Main Content --- //
            // Loading Spinner.
            searchLoadingSpinner = new ActivityIndicator()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                IsVisible = false,
                IsRunning = false,
                Color = Color.FromHex("FE5252"),
            };

            // Songs.
            songsView = new TableView
            {
                RowHeight = 70,
                Intent = TableIntent.Data,
                IsVisible = false,
            };

            // Albums.
            albumsTopLeftLayout = new StackLayout  { };
            albumsTopRightLayout = new StackLayout { };
            albumsGridLayout = new Grid { };
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
            artistsGridLayout = new Grid { };
            artistsGridLayout.Children.Add(artistsTopLeftLayout, 0, 0); // Top left
            artistsGridLayout.Children.Add(artistsTopRightLayout, 1, 0); // Top right
            artistsScrollLayout = new Xamarin.Forms.ScrollView
            {
                IsVisible = false,
                Padding = new Thickness(7, 12, 7, 12),
                Content = artistsGridLayout,
            };

            // Main.
            main = new StackLayout
            {
                IsClippedToBounds = true,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("f2f2f2"),
                Children = {
                    searchLoadingSpinner,
                    songsView,
                    albumsScrollLayout,
                    artistsScrollLayout,
                }
            };


            // --- Footer Content --- //
            trackAlbumImage = new Xamarin.Forms.Image { };
            trackName = new Label { TextColor = Color.FromHex("FEFEFE") };
            trackArtist = new Label { TextColor = Color.FromHex("3f51b4") };
            playOrPause = new Xamarin.Forms.Image { Source = "playOrPause.png"};

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
            trackLoadingSpinner = new ActivityIndicator()
            {
                IsRunning = false,          
                Color = Color.White,
            };
            footerBarLayout = new Grid { };
            footerBarLayout.Children.Add(footerTrackDetailsAndImageLayout, 0, 0);
            footerBarLayout.Children.Add(new BoxView { }, 1, 0);
            footerBarLayout.Children.Add(new BoxView { }, 2, 0);
            footerBarLayout.Children.Add(playOrPause, 3, 0);
            footerBarLayout.Children.Add(trackLoadingSpinner, 3, 0);
            Grid.SetColumnSpan(footerTrackDetailsAndImageLayout, 3);


            // Footer.
            footer = new StackLayout
            {
                VerticalOptions = LayoutOptions.End,
                HeightRequest = 70,
                BackgroundColor = Color.FromHex("7986CB"),
                Padding = new Thickness(4, 6, 4, 6),
                IsVisible = false,
                Children = {
                    footerBarLayout
                }
            };


            // --- Non Content actions. --- //
            tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                if (trackLoadingSpinner.IsRunning == false)
                {
                    if (player.IsPlaying == true)
                    {
                        player.Pause();
                    }
                    else if (player.IsPlaying == false)
                    {
                        player.Start();
                    }
                }
                else
                {
                    return;
                }
            };
            playOrPause.GestureRecognizers.Add(tapGestureRecognizer);


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
            shouldUserGoBack = false;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
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
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
                    }
                    else
                    {
                        foreach (FullTrack track in searchItem.Tracks.Items)
                        {
                            if (track.Album.Images.Count > 0)
                            {
                                UpdateTrackList(track.Album.Images[1].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                            }
                            else
                            {
                                UpdateTrackList("missingTrack.png", track.Name, track.Artists[0].Name, track.PreviewUrl);
                            }
                        }
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
                        songsView.IsVisible = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="album"></param>
        public void SearchAlbumsTracks(SimpleAlbum album)
        {
            shouldUserGoBack = true;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            albumsScrollLayout.IsVisible = false;
            artistsScrollLayout.IsVisible = false;
            songsView.Root.Clear();

            Task.Factory.StartNew(() => albumsTracks = _spotify.GetAlbumTracks(album.Id))
                .ContinueWith(antecendent =>
                {
                    foreach (var track in albumsTracks.Items)
                    {
                        UpdateTrackList(album.Images[1].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                    }
                    searchLoadingSpinner.IsVisible = false;
                    searchLoadingSpinner.IsRunning = false;
                    songsView.IsVisible = true;
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
        }



        /// <summary>
        /// When the user types something into the keyboard and presses search this function is called. The Spotify API is called 
        /// and only told to display tracks. For every song that is returned from Spotify(50), a new ImageCell will be created
        /// displaying the track's name, album name, and album cover image.
        /// </summary>
        public void UpdateTrackList(ImageSource trackImageSource, string trackName, string trackArtist, string trackPreviewUrl)
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
                    TextColor = Color.FromHex("212121"),
                    DetailColor = Color.FromHex("757575"),
                    Command = new Command(() =>
                    {
                        UpdateFooterSong(trackImageSource, trackName, trackArtist);
                        //albumsScrollLayout.HeightRequest = 405; // This fixes issue where footer is pushed down on albums page
                        //artistsScrollLayout.HeightRequest = 405; // This fixes issue where footer is pushed down on artists page
                        //playOrPause.Source = "pause.png";
                        if (player == null)
                        {
                            player = new MediaPlayer();
                            player.SetAudioStreamType (Stream.Music);
                        }
                        if (trackPreviewUrl != null)
                        {
                            trackLoadingSpinner.Opacity = 1;
                            trackLoadingSpinner.IsRunning = true;

                            Task.Factory.StartNew(() => {
                                player.Reset();
                                player.SetDataSource (trackPreviewUrl);
                                player.Prepare();
                                player.Start();
                            })
                            .ContinueWith(antecendent =>
                            {
                                trackLoadingSpinner.Opacity = 0;
                                trackLoadingSpinner.IsRunning = false;
                            },
                            TaskScheduler.FromCurrentSynchronizationContext()
                            );
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





        /// <summary>
        /// 
        /// </summary>
        public void SearchAlbums()
        {
            shouldUserGoBack = false;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            artistsScrollLayout.IsVisible = false;
            songsView.IsVisible = false;
            albumsTopLeftLayout.Children.Clear();
            albumsTopRightLayout.Children.Clear();
            artistsTopLeftLayout.Children.Clear();
            artistsTopRightLayout.Children.Clear();
            songsView.Root.Clear();

            Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Album, 20))
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
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
                    }
                    else
                    {
                        foreach (SimpleAlbum album in searchItem.Albums.Items)
                        {
                            albumIndex++;
                            if (album.Images.Count > 0)
                            {
                                UpdateAlbumList(album, album.Images[1].Url, album.Name);
                            }
                            else
                            {
                                UpdateAlbumList(album, "missingAlbum.png", album.Name);
                            }
                        }
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
                        albumsScrollLayout.IsVisible = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
        }




        /// <summary>
        /// 
        /// </summary>
        public void SearchArtistsAlbums(FullArtist artist)
        {
            shouldUserGoBack = true;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            artistsScrollLayout.IsVisible = false;
            songsView.IsVisible = false;
            albumsTopLeftLayout.Children.Clear();
            albumsTopRightLayout.Children.Clear();
            songsView.Root.Clear();
            albumIndex = 1;

            Task.Factory.StartNew(() => artistsAlbums = _spotify.GetArtistsAlbums(artist.Id))
                .ContinueWith(antecendent =>
                {
                    foreach (var album in artistsAlbums.Items)
                    {
                        albumIndex++;
                        UpdateAlbumList(album, album.Images[1].Url, album.Name);
                    }
                    searchLoadingSpinner.IsVisible = false;
                    searchLoadingSpinner.IsRunning = false;
                    albumsScrollLayout.IsVisible = true;
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
        public void UpdateAlbumList(SimpleAlbum album, ImageSource albumImageSource, string albumName)
        {
            albumFrame = new Frame
            {
                HeightRequest = 225,
                Padding = 0,
                Margin = 0,
                GestureRecognizers = {
                    new TapGestureRecognizer {
                        Command = new Command(() => {
                            SearchAlbumsTracks(album);
                        })
                    },
                },
                Content = new StackLayout
                {
                    Children = {
                        new Xamarin.Forms.Image
                        {
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Aspect = Aspect.AspectFill,
                            Source = albumImageSource,
                        },
                        new Label
                        {
                            Text = albumName,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            FontSize = 13,
                            TextColor = Color.FromHex("212121"),
                            Margin = new Thickness (7, 0, 0, 7),
                        },
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
        public void SearchArtists()
        {
            shouldUserGoBack = false;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            albumsScrollLayout.IsVisible = false;
            songsView.IsVisible = false;
            albumsTopLeftLayout.Children.Clear();
            albumsTopRightLayout.Children.Clear();
            artistsTopLeftLayout.Children.Clear();
            artistsTopRightLayout.Children.Clear();
            songsView.Root.Clear();

            Task.Factory.StartNew(() => searchItem = _spotify.SearchItems(searchBar.Text, SearchType.Artist, 20))
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
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
                    }
                    else
                    {
                        foreach (FullArtist artist in searchItem.Artists.Items)
                        {
                            artistIndex++;
                            if (artist.Images.Count > 0)
                            {
                                UpdateArtistList(artist, artist.Images[1].Url, artist.Name, artist.Popularity);
                            }
                            else
                            {
                                UpdateArtistList(artist, "missingArtist.png", artist.Name, artist.Popularity);
                            }
                        }
                        searchLoadingSpinner.IsVisible = false;
                        searchLoadingSpinner.IsRunning = false;
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
        public void UpdateArtistList(FullArtist artist, ImageSource artistImageSource, string artistName, int artistPopularity)
        {
            var artistPopularityString = artistPopularity.ToString();
            artistFrame = new Frame
            {
                HeightRequest = 250,
                Padding = 0,
                Margin = 0,
                GestureRecognizers = {
                    new TapGestureRecognizer {
                        Command = new Command(() => {
                            SearchArtistsAlbums(artist);
                        })
                    },
                },
                Content = new StackLayout
                {
                    Children = {
                        new Xamarin.Forms.Image
                        {
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Aspect = Aspect.AspectFill,
                            Source = artistImageSource,
                        },
                        new Label
                        {
                            Text = artistName,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            TextColor = Color.FromHex("212121"),
                            FontSize = 12,
                            Margin = new Thickness (7, 0, 0, 0),
                        },
                        new Label
                        {
                            Text = "Popularity: " + artistPopularityString,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            TextColor = Color.FromHex("757575"),
                            FontSize = 10,
                            Margin = new Thickness (7, -3, 0, 7),
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
            if (shouldUserGoBack == true)
            {
                if (songsView.IsVisible == true)
                {
                    shouldUserGoBack = false;
                    albumsScrollLayout.IsVisible = true;
                    songsView.IsVisible = false;
                }

                else if (albumsScrollLayout.IsVisible == true)
                {
                    shouldUserGoBack = false;
                    artistsScrollLayout.IsVisible = true;
                    albumsScrollLayout.IsVisible = false;
                }
            }
            return true;
        }
    }
}
