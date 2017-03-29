using Isic.Debugger;
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
        MonitorSettings monitor;
        public MonitorInformationPage(MonitorSettings _monitor)
        {
            this.monitor = _monitor;
            IsicDebug.DebugGeneral(String.Format("Initiliazed Monitor Information Page with Monitor at address: {0}", monitor.MonAddr));

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
            NameInfo.Text = "DuraMON 24 Glassé";
        
            RNInfo.Text = "AA00TDX";
            
            SNInfo.Text = "76A1609001";
            
            FirmwareInfo.Text = "Transas 24'' Config";
            
            BaudInfo.Text = "19K2";
            
            TempInfo.Text = "25.2";
            TempInfo.Text += "°C";
            
            LightInfo.Text = "60.520";

            TimeInfo.Text = "22.5";
            TimeInfo.Text += " h.";

        }
    }
}
