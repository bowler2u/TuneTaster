using Android.Media;
using Android.Widget;
using Newtonsoft.Json.Linq;
using RestSharp;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TuneTaster
{
    class MainPage : ContentPage
    {   
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

        // Music functionality/Spotify API.
        private static string accessToken;
        SpotifyWebAPI _spotify;
        SearchItem searchItem;
        Paging<SimpleTrack> albumsTracks;
        Paging<SimpleAlbum> artistsAlbums;
        protected MediaPlayer player;

        // Extras
        bool userSearchedAlbums;
        bool userSearchedArtists;
        int albumIndex;
        int artistIndex;
        Toast noPreviewToast;
        Toast noresultsToast;
        TapGestureRecognizer tapGestureRecognizer;

        public MainPage()
        {
            // Initalize Spotify Authentication.
            var client = new RestClient("https://accounts.spotify.com/api/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "Basic MzMzZGUzOGU0YTI3NDRmY2JlMzNlYWU4YTM5ZDhiY2U6MTQxNjc1ZmY1ZTI3NDE4NmI0NWVmNDg0OTNhOWE1YmY=");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            // Create a json object from the response context and grab only the access token and store it as a variable. This token is required for
            // doing track, album and artist requests.
            JObject contentObject = JObject.Parse(response.Content);
            object accessTokenObject = contentObject["access_token"];
            accessToken = accessTokenObject.ToString();

            // Initalize the Spotify API, plugging in the generated token.
            _spotify = new SpotifyWebAPI
            {
                 TokenType = "Bearer",
                 AccessToken = accessToken,
            };

      
            // --- Header Content --- //
            // Search Bar.
            searchBar = new SearchBar
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                MinimumWidthRequest = 200,
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
                HorizontalOptions = LayoutOptions.FillAndExpand,
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
            playOrPause = new Xamarin.Forms.Image { Source = "playOrPause.png", VerticalOptions = LayoutOptions.FillAndExpand};

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
                HeightRequest = 65,
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
        /// Clears all views that may have been showing and starts a search task that will call the Spotify API in the background as the loader icon spins for the user.
        /// </summary>
        public void SearchTracks()
        {
            userSearchedAlbums = false;
            userSearchedArtists = false;
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
                            int imagesCount = track.Album.Images.Count;
                            if (imagesCount > 0)
                            {
                                UpdateTrackList(track.Album.Images[imagesCount/2].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                            }
                            else
                            {
                                UpdateTrackList("missingTrack.jpg", track.Name, track.Artists[0].Name, track.PreviewUrl);
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
        /// Grabs all the tracks from the album the user just selected.
        /// </summary>
        /// <param name="album">The album the user just selected, passed in so I can grab the cover art.</param>
        public void SearchAlbumsTracks(SimpleAlbum album)
        {
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
                        int imagesCount = album.Images.Count;
                        UpdateTrackList(album.Images[imagesCount/2].Url, track.Name, track.Artists[0].Name, track.PreviewUrl);
                    }
                    searchLoadingSpinner.IsVisible = false;
                    searchLoadingSpinner.IsRunning = false;
                    songsView.IsVisible = true;
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
        }


        /// <summary>
        /// The Spotify API is called and only told to display 50 tracks. For every song that is returned from Spotify, a new ImageCell will be created
        /// displaying the track's name, album name, and album cover image.
        /// </summary>
        /// <param name="trackImageSource">The track's album art.</param>
        /// <param name="trackName">Name of the track.</param>
        /// <param name="trackArtist">The artist that sings the track.</param>
        /// <param name="trackPreviewUrl">The 30 sec preview url that will get played when the user selects the track.</param>
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
                        UpdateFooterSong(trackImageSource, trackName, trackArtist); // Pass the data from the selected image cell to the update footer method.
                        if (player == null)
                        {
                            player = new MediaPlayer();
                            player.SetAudioStreamType (Android.Media.Stream.Music);
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
        /// Clears all views that may have been showing and starts a search task that will call the Spotify API in the background as the loader icon spins for the user.
        /// </summary>
        public void SearchAlbums()
        {
            userSearchedAlbums = true;
            userSearchedArtists = false;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            artistsScrollLayout.IsVisible = false;
            songsView.IsVisible = false;
            albumsTopLeftLayout.Children.Clear();
            albumsTopRightLayout.Children.Clear();
            artistsTopLeftLayout.Children.Clear();
            artistsTopRightLayout.Children.Clear();
            songsView.Root.Clear();
            albumIndex = 1;

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
                            var imagesCount = album.Images.Count;
                            if (imagesCount > 0)
                            {
                                UpdateAlbumList(album, album.Images[imagesCount/2].Url, album.Name);
                            }
                            else
                            {
                                UpdateAlbumList(album, "missingAlbum.jpg", album.Name);
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
        /// Grabs all the albums from the artist the user just selected.
        /// </summary>
        /// <param name="artist">The individual artist.</param>
        public void SearchArtistsAlbums(FullArtist artist)
        {
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
                        var imagesCount = album.Images.Count;
                        UpdateAlbumList(album, album.Images[imagesCount/2].Url, album.Name);
                    }
                    searchLoadingSpinner.IsVisible = false;
                    searchLoadingSpinner.IsRunning = false;
                    albumsScrollLayout.IsVisible = true;
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
        }


        /// <summary>
        /// The spotify API is called, limiting only 20 results, and creating new Frame views within a stack layout.
        /// </summary>
        /// <param name="album">A singular album.</param>
        /// <param name="albumImageSource">The source of the album's art.</param>
        /// <param name="albumName">The name of the album.</param>
        public void UpdateAlbumList(SimpleAlbum album, ImageSource albumImageSource, string albumName)
        {
            albumFrame = new Frame
            {
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
                            HeightRequest = 200,
                            Aspect = Aspect.Fill,
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
            // Places the Frame to the left. (if the index is an even number)
            if (albumIndex % 2 == 0)
            {
                albumsTopLeftLayout.Children.Add(albumFrame);
            }
            // Places the Frame to the right. (if the index is an odd number)
            else
            {
                albumsTopRightLayout.Children.Add(albumFrame);
            }
        }


        /// <summary>
        /// Clears all views that may have been showing and starts a search task that will call the Spotify API in the background as the loader icon spins for the user.
        /// </summary>
        public void SearchArtists()
        {
            userSearchedArtists = true;
            userSearchedAlbums = false;
            searchLoadingSpinner.IsVisible = true;
            searchLoadingSpinner.IsRunning = true;
            albumsScrollLayout.IsVisible = false;
            songsView.IsVisible = false;
            albumsTopLeftLayout.Children.Clear();
            albumsTopRightLayout.Children.Clear();
            artistsTopLeftLayout.Children.Clear();
            artistsTopRightLayout.Children.Clear();
            songsView.Root.Clear();
            artistIndex = 1;

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
                            int imagesCount = artist.Images.Count;
                            if (imagesCount > 0)
                            {
                                UpdateArtistList(artist, artist.Images[imagesCount/2].Url, artist.Name, artist.Popularity);
                            }
                            else
                            {
                                UpdateArtistList(artist, "missingArtist.jpg", artist.Name, artist.Popularity);
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
        /// Similar to updating albums, the spotify API is called, limiting only 20 results, and creating new Frame views within a stack layout.
        /// </summary>
        /// <param name="artist">The artist.</param>
        /// <param name="artistImageSource">The artist's image url.</param>
        /// <param name="artistName"> The name of the artist.</param>
        public void UpdateArtistList(FullArtist artist, ImageSource artistImageSource, string artistName, int artistPopularity)
        {
            var artistPopularityString = artistPopularity.ToString();
            artistFrame = new Frame
            {
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
                            HeightRequest = 200,
                            Aspect = Aspect.Fill,
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
            // Places the Frame to the left. (if the index is an even number)
            if (artistIndex % 2 == 0)
            {
                artistsTopLeftLayout.Children.Add(artistFrame);
            }
            // Places the Frame to the right. (if the index is an odd number)
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
        /// Handle back button press events. The user should never be able to go back if they just searched something. However, if they select an album for example,
        /// they should be able to press the back button and return to the albums screen.
        /// </summary>
        /// <returns>Prevents the app from closing.</returns>
        protected override bool OnBackButtonPressed()
        {        

            if (userSearchedAlbums == true)
            {
                if (songsView.IsVisible == true)
                {
                    albumsScrollLayout.IsVisible = true;
                    songsView.IsVisible = false;
                }
            }

            else if (userSearchedArtists == true)
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
            }
            return true;
        }
    }
}