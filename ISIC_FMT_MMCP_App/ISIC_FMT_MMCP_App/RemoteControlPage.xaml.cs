using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class RemoteControlPage : ContentPage
    {
        public RemoteControlPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        void OnSettingsClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new DeviceList());
        }
    }
}
