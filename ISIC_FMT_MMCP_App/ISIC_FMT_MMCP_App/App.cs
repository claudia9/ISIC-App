using Acr.UserDialogs;
using Android;
using Android.Content;
using Android.Preferences;
using Isic.Debugger;
using Plugin.BLE.Abstractions.Contracts;
using System;
using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new DeviceList());
            //CNC@ISIC 28-03-17 Changed in order to deploy to show as DEMO - Not connecting to any device!
            //MainPage = new NavigationPage(new RemoteControlPage(null));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            InitializeProperties(); 
        }

        private void InitializeProperties()
        {
            AssignValues("Mon1Addr", (byte)0);
            AssignValues("Mon2Addr", (byte)0);
            AssignValues("Mon3Addr", (byte)0);
            AssignValues("MonAllAddr", (byte)250);
            AssignValues("Baud", 0);
            AssignValues("LastBluetooth", "");
            AssignValues("Show_Instructions_MonAddr", true);
        }

        private void AssignValues(string key, object value)
        {
            if (!Application.Current.Properties.ContainsKey(key))
            {
                Application.Current.Properties[key] = value;
            }
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
