using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;

namespace ISIC_FMT_MMCP_App.Droid
{
    [Activity(Label = "ISIC App", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            //retrieveSettings();
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            UserDialogs.Init(this);

            LoadApplication(new App());
        }

        private void retrieveSettings()
        {
            //Retrieve
            var prefs = Application.Context.GetSharedPreferences("IsicApp", Android.Content.FileCreationMode.Private);
            var mon1Addr = prefs.GetString("mon1Addr", null);
            var mon2Addr = prefs.GetString("mon2Addr", null);
            var mon3Addr = prefs.GetString("mon3Addr", null);

            var baud = prefs.GetString("baud", null);

            //Show a toast with the values
            RunOnUiThread(() => Toast.MakeText(this, mon1Addr + mon2Addr + mon3Addr + baud, ToastLength.Long).Show());
        }

        protected override void OnDestroy()
        {
            //saveSettings();
            base.OnDestroy();
        }

        private void saveSettings()
        {
            //Store
            var prefs = Application.Context.GetSharedPreferences("IsicApp", Android.Content.FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("mon1Addr", "1");
            prefEditor.PutString("mon2Addr", "140");
            prefEditor.PutString("mon3Addr", "33");
            prefEditor.PutString("baud", "19K2");

            prefEditor.Commit();
        }
    }
}

