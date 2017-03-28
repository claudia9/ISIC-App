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
            Label Name = NameInfo;
            Name.Text = "DuraMON 24 Glassé";

            Label RN = RNInfo;
            RN.Text = "AA00TDX";

            Label SN = SNInfo;
            SN.Text = "76A1609001";

            Label Firmware = FirmwareInfo;
            Firmware.Text = "Transas 24'' Config";

            Label Baud = BaudInfo;
            Baud.Text = "19K2";

            Label Temp = TempInfo;
            Temp.Text = "25.2";
            Temp.Text += "°C";

            Label Light = LightInfo;
            Light.Text = "60.520";

            Label Time = TimeInfo;
            Time.Text = "22.5";
            Time.Text += " h.";

        }
    }
}
