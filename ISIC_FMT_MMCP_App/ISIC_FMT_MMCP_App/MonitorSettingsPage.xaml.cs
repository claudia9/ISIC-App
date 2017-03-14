using Isic.Debugger;
using Plugin.BLE;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorSettingsPage : ContentPage
    {
        private Dictionary<MonitorIdentifier, MonitorSettings> monitors { get; set; }

        public MonitorSettingsPage()
        {
            InitializeScreen();
            InitializeComponent();
            InitializeDictionary();
            InitializeComboBoxes();

            var ble = CrossBluetoothLE.Current;
            if (ble.State == Plugin.BLE.Abstractions.Contracts.BluetoothState.Off)
            {

            }

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

        private void SetPreferences()
        {
            //SetPreferences -> Need implementation
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

            Picker baud = Baud;
            baud.Items.Add("9K6");
            baud.Items.Add("19K2");
            baud.Items.Add("115K2");
            baud.Items.Add("460K8");

            Mon1Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor1].MonAddr;
            Mon2Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor2].MonAddr;
            Mon3Addr.SelectedIndex = monitors[MonitorIdentifier.Monitor3].MonAddr;

            Mon1Addr.SelectedIndexChanged += Mon1Addr_SelectedIndexChanged;
            Mon2Addr.SelectedIndexChanged += Mon2Addr_SelectedIndexChanged;
            Mon3Addr.SelectedIndexChanged += Mon3Addr_SelectedIndexChanged;
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
            monitors[monIdentifier].MonAddr = (Byte)(sender as Picker).SelectedIndex;

            IsicDebug.DebugMonitor("Setting Monitor" + monIdentifier + " addres: " + monitors[monIdentifier].MonAddr);
        }
        #endregion

    }

    /*private void retrieveSettings()
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
    
             private void SaveSettings()
        {
            //Store
            var prefs = Application.Context.GetSharedPreferences("IsicApp", Android.Content.FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("mon1Addr", "1");
            prefEditor.PutString("mon2Addr", "140");
            prefEditor.PutString("mon3Addr", "33");
            prefEditor.PutString("baud", "19K2");

            prefEditor.Commit();
        }*/
}
