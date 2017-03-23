using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorInformationPage : ContentPage
    {
        public MonitorInformationPage()
        {
            InitializeScreen();
            InitializeComponent();
            InitializeButtons();
            InitializeMonitorInfo();
        }

        private void InitializeButtons()
        {
            BackButton.Clicked += BackButton_Clicked;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void InitializeMonitorInfo()
        {
            Label Name = NameInfo;
            Name.Text = "Claudia";

            Label RN = RNInfo;
            RN.Text = "AA00WWN-A";

            Label Temp = TempInfo;
            Temp.Text = "25.2";
            Temp.Text += "°C";

            Label Time = TimeInfo;
            Time.Text = "2.2";
            Time.Text += "t.";
        }
    }
}
