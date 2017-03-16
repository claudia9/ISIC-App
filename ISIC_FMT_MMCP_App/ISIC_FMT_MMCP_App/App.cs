using Acr.UserDialogs;
using Android.Content;
using Android.Preferences;
using Isic.Debugger;
using System;
using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class App : Application
    {
        public App()
        {

            MainPage = new NavigationPage(new DeviceList());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            InitializeProperties(); 
        }

        private void InitializeProperties()
        {
            Application.Current.Properties["Mon1Addr"] = (Byte)0;
            Application.Current.Properties["Mon2Addr"] = (Byte)0;
            Application.Current.Properties["Mon3Addr"] = (Byte)0;
            Application.Current.Properties["MonAllAddr"] = (Byte)255;
            Application.Current.Properties["Baud"] = 2;
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
