using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace TuneTaster
{
    public partial class App : Application
    {
        public App()
        {
            // Limit theading amounts for loading large amounts of data (especially album images).
            System.Threading.ThreadPool.SetMinThreads(70, 70);
            System.Threading.ThreadPool.SetMaxThreads(100, 100);

            // Start the MainPage.
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
