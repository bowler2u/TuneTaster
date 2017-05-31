using Xamarin.Forms;
using System;

namespace TuneTaster
{
    public class MainPage : TabbedPage
    {
        public MainPage()
        {
            Children.Add(new SongsPage());
            Children.Add(new AlbumsPage());
            Children.Add(new ArtistsPage());
        }
    }
}






