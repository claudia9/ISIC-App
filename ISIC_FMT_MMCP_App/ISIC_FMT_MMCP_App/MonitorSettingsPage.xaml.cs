using Acr.UserDialogs;
using Isic.Debugger;
using Isic.ViewModels;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorSettingsPage : ContentPage
    {
        private Dictionary<MonitorIdentifier, MonitorSettings> monitors { get; set; }

        private IAdapter CurrentAdapter;
        private ICharacteristic CurrentCharacteristic;
        private LoadingViewModel saveViewModel;

        public MonitorSettingsPage(ICharacteristic characteristic)
        {
            InitializeScreen();
            InitializeComponent();
            InitializeButtons();
            InitializeBluetooth(characteristic);
            InitializeDictionary();
            InitializeComboBoxes();

            IsicDebug.DebugGeneral("On MonitorSettingsPage");
        }
        private void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void InitializeButtons()
        {

            saveViewModel = new LoadingViewModel(UserDialogs.Instance);

            //Binding SavePreferences
            this.BindingContext = saveViewModel;

            Back.Clicked += Back_Clicked;
            AdvSettingsButton.Clicked += AdvSettingsButton_Clicked;
        }
        private void InitializeBluetooth(ICharacteristic characteristic)
        {
            CurrentCharacteristic = characteristic;

            var ble = CrossBluetoothLE.Current;
            CurrentAdapter = ble.Adapter;
            if (ble.State == BluetoothState.Off)
            {
                UserDialogs.Instance.Toast("Please, turn on your Bluetooth to experience all the features of this DEMO.");
            }

        }

        private void InitializeDictionary()
        {
            monitors = new Dictionary<MonitorIdentifier, MonitorSettings>();
            monitors[MonitorIdentifier.Monitor1] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor2] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor3] = new MonitorSettings();
            monitors[MonitorIdentifier.MonitorBroadcast] = new MonitorSettings() { MonAddr = 0xFF };

        }

        private void InitializeComboBoxes()
        {
            for (int i = 0; i < 255; i++)
            {
                Mon1Addr.Items.Add(i.ToString());
                Mon2Addr.Items.Add(i.ToString());
                Mon3Addr.Items.Add(i.ToString());
            }

            if (Application.Current.Properties.ContainsKey("Mon1Addr"))
            {
                try
                {
                    Mon1Addr.SelectedIndex = (int)Application.Current.Properties["Mon1Addr"];
                }
                catch (Exception e)
                {
                    IsicDebug.DebugException(String.Format("Giving address to comboboxes from the phone Properties", e));
                }
            }
            if (Application.Current.Properties.ContainsKey("Mon2Addr"))
            {
                try
                {
                    Mon2Addr.SelectedIndex = (int)Application.Current.Properties["Mon2Addr"];
                }
                catch (Exception e)
                {
                    IsicDebug.DebugException(String.Format("Giving address to comboboxes from the phone Properties", e));
                }
            }
            if (Application.Current.Properties.ContainsKey("Mon3Addr"))
            {
                try
                {
                    Mon3Addr.SelectedIndex = (int)Application.Current.Properties["Mon3Addr"];
                }
                catch (Exception e)
                {
                    IsicDebug.DebugException(String.Format("Giving address to comboboxes from the phone Properties", e));
                }
            }

            Mon1Addr.SelectedIndexChanged += Mon1Addr_SelectedIndexChanged;
            Mon2Addr.SelectedIndexChanged += Mon2Addr_SelectedIndexChanged;
            Mon3Addr.SelectedIndexChanged += Mon3Addr_SelectedIndexChanged;

            Baud.Items.Add("9K6");
            Baud.Items.Add("19K2");
            Baud.Items.Add("115K2");
            Baud.Items.Add("460K8");

            if (Application.Current.Properties["Baud"] != null)
            {
                Baud.SelectedIndex = (int)Application.Current.Properties["Baud"];
            }

            Baud.SelectedIndexChanged += Baud_SelectedIndexChanged;

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
            switch (monIdentifier)
            {
                case MonitorIdentifier.Monitor1:
                    Application.Current.Properties["Mon1Addr"] = (int)(sender as Picker).SelectedIndex;
                    break;
                case MonitorIdentifier.Monitor2:
                    Application.Current.Properties["Mon2Addr"] = (int)(sender as Picker).SelectedIndex;
                    break;
                case MonitorIdentifier.Monitor3:
                    Application.Current.Properties["Mon3Addr"] = (int)(sender as Picker).SelectedIndex;
                    break;
            }


            monitors[monIdentifier].MonAddr = (Byte)(sender as Picker).SelectedIndex;

            IsicDebug.DebugMonitor(String.Format("Setting property {0} to {1}", monIdentifier.ToString(), (sender as Picker).SelectedIndex));
            IsicDebug.DebugMonitor(String.Format("Setting Monitor {0}, address: {1}", monIdentifier, monitors[monIdentifier].MonAddr));
        }
        #endregion


        private void Back_Clicked(object sender, EventArgs e)
        {
            SetPreferences();
            Navigation.PopAsync();
        }
        private async void SetPreferences()
        {
            await Application.Current.SavePropertiesAsync();
        }


        private async void AdvSettingsButton_Clicked(object sender, EventArgs e)
        {
            //UserDialogs.Instance.Toast("This feature is not available for this DEMO version");
            await Navigation.PushAsync(new ChangeMonitorAddressPage(CurrentCharacteristic));
        }

    }
}
