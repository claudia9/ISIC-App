using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new DeviceList());
            MainPage.SetValue(NavigationPage.BarBackgroundColorProperty, Color.FromHex("#4D4D4D"));
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
