using Acr.UserDialogs;
using Isic.Debugger;
using Isic.ViewModels;
using Plugin.BLE;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorSettingsPage : ContentPage
    {
        private Dictionary<MonitorIdentifier, MonitorSettings> monitors { get; set; }

        LoadingViewModel saveViewModel;

        public MonitorSettingsPage()
        {
            saveViewModel = new LoadingViewModel(UserDialogs.Instance);

            InitializeScreen();
            InitializeComponent();
            InitializeDictionary();
            InitializeComboBoxes();

            var ble = CrossBluetoothLE.Current;
            if (ble.State == Plugin.BLE.Abstractions.Contracts.BluetoothState.Off)
            {
                UserDialogs.Instance.Toast("Your bluetooth has been disconnected, please, connect it again to proceed.");
            }
            //Binding ScanAllbutton
            this.BindingContext =saveViewModel;

            Back.Clicked += Back_Clicked;

            IsicDebug.DebugGeneral("On MonitorSettingsPage");
            
            //retrieveSettings();
        }

        private void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void Back_Clicked(object sender, EventArgs e)
        {
            SetPreferences();
            Navigation.PopAsync();
        }

        private async void SetPreferences()
        {

            await Application.Current.SavePropertiesAsync();
        }

        private void InitializeDictionary()
        {
            monitors = new Dictionary<MonitorIdentifier, MonitorSettings>();
            monitors[MonitorIdentifier.Monitor1] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor2] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor3] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor4] = new MonitorSettings();
            monitors[MonitorIdentifier.MonitorBroadcast] = new MonitorSettings() { MonAddr = 0xFF };

        }

        private void InitializeComboBoxes()
        {
            Picker mon1 = Mon1Addr;
            Picker mon2 = Mon2Addr;
            Picker mon3 = Mon3Addr;
            for (int i = 0; i < 255; i++)
            {
                mon1.Items.Add(i.ToString());
                mon2.Items.Add(i.ToString());
                mon3.Items.Add(i.ToString());
            }

            Mon1Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor1].MonAddr;
            Mon2Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor2].MonAddr;
            Mon3Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor3].MonAddr;

            Mon1Addr.SelectedIndexChanged += Mon1Addr_SelectedIndexChanged;
            Mon2Addr.SelectedIndexChanged += Mon2Addr_SelectedIndexChanged;
            Mon3Addr.SelectedIndexChanged += Mon3Addr_SelectedIndexChanged;


            Picker baud = Baud;
            baud.Items.Add("9K6");
            baud.Items.Add("19K2");
            baud.Items.Add("115K2");
            baud.Items.Add("460K8");

            if(Application.Current.Properties["Baud"] != null) {
                baud.SelectedIndex = (int)Application.Current.Properties["Baud"];
            }

            baud.SelectedIndexChanged += Baud_SelectedIndexChanged;

        }


        private void Baud_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetBaud(sender);
        }

        private void SetBaud(object sender)
        {
            Application.Current.Properties["Baud"] = (int)(sender as Picker).SelectedIndex;
            IsicDebug.DebugMonitor(String.Format("Setting property Baud to {0}", (int)(sender as Picker).SelectedIndex));
        }
        #region Set Addresses from ComboBoxes
        private void Mon1Addr_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMonitorAddress(sender, MonitorIdentifier.Monitor1);
        }
        private void Mon2Addr_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMonitorAddress(sender, MonitorIdentifier.Monitor2);
        }

        private void Mon3Addr_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMonitorAddress(sender, MonitorIdentifier.Monitor3);
        }

        private void SetMonitorAddress(object sender, MonitorIdentifier monIdentifier)
        {
            Application.Current.Properties[monIdentifier.ToString()] = (Byte)(sender as Picker).SelectedIndex;
            monitors[monIdentifier].MonAddr = (Byte)(sender as Picker).SelectedIndex;

            IsicDebug.DebugMonitor(String.Format("Setting property {0} to {1}", monIdentifier.ToString(), (sender as Picker).SelectedIndex));
            IsicDebug.DebugMonitor(String.Format("Setting Monitor {0}, address: {1}", monIdentifier, monitors[monIdentifier].MonAddr));
            //IsicDebug.DebugGeneral(String.Format("General preference - Mon1Addr = {0}", Application.Current.Properties["Mon1Addr"]));
        }
        #endregion

    }
}
