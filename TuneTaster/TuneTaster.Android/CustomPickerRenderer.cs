using System;
using Android.Widget;
using Android.Text;
using G = Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Text.Style;

[assembly: ExportRenderer(typeof(Picker), typeof(TuneTaster.CustomPickerRenderer))]

namespace TuneTaster
{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> args)
        {
            base.OnElementChanged(args);
            Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }
    }
}