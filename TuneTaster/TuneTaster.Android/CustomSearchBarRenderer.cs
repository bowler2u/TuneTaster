using System;
using Android.Widget;
using Android.Text;
using G = Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Text.Style;

[assembly: ExportRenderer(typeof(SearchBar), typeof(TuneTaster.CustomSearchBarRenderer))]

namespace TuneTaster
{
    public class CustomSearchBarRenderer : SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> args)
        {
            base.OnElementChanged(args);

            // Get native control (background set in shared code, but can use SetBackgroundColor here)
            SearchView searchView = (base.Control as SearchView);
            searchView.SetInputType(InputTypes.ClassText | InputTypes.TextVariationNormal);

            // Set the magnifying classe's color to white.
            var searchIconId = searchView.Resources.GetIdentifier("android:id/search_mag_icon", null, null);
            var searchPlateIcon = searchView.FindViewById(searchIconId);
            (searchPlateIcon as ImageView).SetColorFilter(G.Color.White, G.PorterDuff.Mode.SrcIn);

            // Eliminate the grey underline bar for the search bar
            var plateId = searchView.Context.Resources.GetIdentifier("android:id/search_plate", null, null);
            var plate = Control.FindViewById(plateId);
            plate.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }
    }
}