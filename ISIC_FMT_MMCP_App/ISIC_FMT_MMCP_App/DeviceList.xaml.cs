using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using System;
using Acr.UserDialogs;
using Isic.Debugger;
using Isic.ViewModels;
using System.Threading.Tasks;

namespace ISIC_FMT_MMCP_App
{
    public partial class DeviceList : ContentPage
    {
        //Collection of IDevice to collect each device encountered
        ObservableCollection<IDevice> DevicesList;

        /// <summary>
        /// 
        /// </summary>
        LoadingViewModel loadViewModel;

        //Bluetooth Settings
        IBluetoothLE Ble;
        IAdapter Adapter;
        IDevice CurrentDevice;

        /// <summary>
        /// 
        /// </summary>
        public DeviceList()
        {
            DevicesList = new ObservableCollection<IDevice>();
            loadViewModel = new LoadingViewModel(UserDialogs.Instance);

            Ble = CrossBluetoothLE.Current;
            Adapter = Ble.Adapter;
            Adapter.ScanTimeout = 10000;

            //Delete Navigation Bar
            InitializeScreen();

            //Automatically written to get the XAML
            InitializeComponent();

            //Binding ScanAllbutton
            this.BindingContext = loadViewModel;

            CheckAvailabilityBluetooth(Ble.State);

            Ble.StateChanged += (s, e) =>
            {
                IsicDebug.DebugBluetooth(String.Format("Bluetooth LE changed it's state from {0} to: {1}", e.NewState, e.OldState));
                if (e.NewState == BluetoothState.Off)
                {
                    UserDialogs.Instance.Toast(new ToastConfig("Your Bluetooth has turned off, please turn it on againg before proceding"));
                }
                else if (e.NewState == BluetoothState.On)
                {
                    ToastConfig toast = new ToastConfig("Your Bluetooth has turned on, enjoy the features of this app ;)");
                    UserDialogs.Instance.Toast(toast);
                }
            };


            //Bluetooth Connection
            InitiliazeDefaultBluetooth();


            bool blockScan = false;
            //Event handler for the Scan Button
            ScanAllButton.Clicked += async (sender, e) =>
            {
                if (blockScan == false)
                {
                    blockScan = true;
                    CheckAvailabilityBluetooth(Ble.State);
                    DeleteOldConnections();
                    await InitiliazeBluetooth();
                    blockScan = false;
                }
            };

            listView.ItemSelected += OnItemSelected;
            
        }

 
        private void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            DevicesList.Clear();
        }

        private void CheckAvailabilityBluetooth(BluetoothState state)
        {
            if (state == BluetoothState.Off)
            {
                DisplayAlert("Bluetooth is off", "Please, turn on the Bluetooth on the device before proceed", "Ok");
            }
            else if (state == BluetoothState.Unavailable)
            {
                DisplayAlert("Bluetooth not supported", "This device doesn't support Bluetooth LE", "Ok");
            }
            else if (state == BluetoothState.Unauthorized)
            {
                DisplayAlert("Bluetooth permissions", "Please, allow the app to have access to the Bluetooth through the settings in your device", "Ok");
            }
        }

        private void DeleteOldConnections()
        {
            //Delete any rests of the last Bluetooth scans
            DevicesList.Clear();

            if (Adapter.ConnectedDevices.Count >= 1)
            {
                foreach (IDevice device in Adapter.ConnectedDevices)
                {
                    Adapter.DisconnectDeviceAsync(device);
                }
            }

            if (CurrentDevice != null)
            {
                CurrentDevice = null;
            }
        }


        private async void InitiliazeDefaultBluetooth()
        {
            //If we ever can pair to the devices -> Use this function to connect to the paired ones.!
            /*var systemDevices = adapter.GetSystemConnectedOrPairedDevices();
            foreach(var knowndevice in systemDevices)
            {
                try
                {
                    currentDevice = await adapter.ConnectToKnownDeviceAsync(knowndevice.Id);
                    Debug.WriteLine("Connected to known device: " + knowndevice.Name);
                } catch (Exception ex)
                {
                    Debug.WriteLine("Could not connect to known or paired device" + ex.Message);
                }
            }*/
            if (Adapter != null)
            {
                try
                {
                    CurrentDevice = await Adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-f0c77f1c2065"));        //bleCACA
                    //CurrentDevice = await Adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-a81b6aaec165"));        //Isic Demo2
                    if (CurrentDevice != null)
                    {

                        IsicDebug.DebugBluetooth(String.Format("Connected to device: {0}", CurrentDevice.Name));
                        await Navigation.PushAsync(new RemoteControlPage(CurrentDevice));
                    }
                }
                catch (DeviceConnectionException e)
                {
                    IsicDebug.DebugException(String.Format("Could not connect to default device!! Exception: {0}", e));
                }
                catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("General exception thrown while trying to connect to device. Exception {0}", ex));
                }
            }

        }

        private async Task InitiliazeBluetooth()
        {
            IsicDebug.DebugBluetooth(String.Format("BluetoothLE initiliased correctly."));

            if (Adapter != null)
            {
                //Start scanning
                StartScanning();

                //Initiliase list of devices
                Adapter.DeviceDiscovered += (s, a) =>
                {
                    if (!DevicesList.Contains(a.Device) && a.Device.Name != "" && a.Device.Name != null)
                    {
                        DevicesList.Add(a.Device);
                        IsicDebug.DebugBluetooth(String.Format("Device found: {0}", a.Device.Name));
                        IsicDebug.DebugBluetooth(String.Format("Device id: {0}", a.Device.Id));
                    }
                };
            }

            //Event handlers to show the List of devices once found
            listView.ItemsSource = DevicesList;

        }

        public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            CurrentDevice = e.SelectedItem as IDevice;
            if (CurrentDevice == null)
            {
                return;
            }

            if (Adapter != null)
            {
                StopScanning();

                ConnectToDevice();
            }
            else
            {
                IsicDebug.DebugBluetooth(String.Format("Adapter is null, checking availability of bluetooth"));
                if (Ble != null)
                {
                    CheckAvailabilityBluetooth(Ble.State);
                }
            }


        }

        public async void ConnectToDevice()
        {
            if (CurrentDevice != null)
            {
                IsicDebug.DebugBluetooth(String.Format("Selected device: {0}", CurrentDevice.Name));
                try
                {
                    await Adapter.ConnectToDeviceAsync(CurrentDevice);
                    IsicDebug.DebugBluetooth(String.Format("Connected to device {0}", CurrentDevice.Name));
                    PushRemoteControlPage();
                }
                catch (DeviceConnectionException ex)
                {
                    IsicDebug.DebugException(String.Format("Could not connect to device. Exception: {0}", ex));
                }
                catch (Exception exep)
                {
                    IsicDebug.DebugException(String.Format("General exception thrown when trying to connect to device. {0}", exep));
                }
            }
        }

        private async void PushRemoteControlPage()
        {
            if (CurrentDevice != null && Adapter.ConnectedDevices.Contains(CurrentDevice))
            {
                await Navigation.PushAsync(new RemoteControlPage(CurrentDevice));
                DevicesList.Clear();
                CurrentDevice = null;
            }
            else
            {
                UserDialogs.Instance.Toast("The connection did not end succesfully, please, try again.");
            }

        }

        async void StartScanning()
        {
            IsicDebug.DebugBluetooth(String.Format(" ------- Start scanning -----------"));
            DevicesList.Clear();

            if (Adapter.IsScanning)
            {
                await Adapter.StopScanningForDevicesAsync();
                IsicDebug.DebugBluetooth(String.Format("Adapter already trying to scan. STOPPING LAST SCAN and TRY AGAIN"));
                await Adapter.StartScanningForDevicesAsync();
            }
            else
            {
                IsicDebug.DebugBluetooth(String.Format("Cleared list of devices and starting scanning."));
                await Adapter.StartScanningForDevicesAsync();
            }
            Adapter.ScanTimeoutElapsed += (s, e) =>
            {
                if (DevicesList.Count() == 0)
                {
                    StopScanning();
                    DisplayAlert("No devices found", "No devices have been found", "OK");
                }
                else
                {
                    StopScanning();
                    //DisplayAlert("Time out", "Time out has been reached. If the target device do not appear, try again", "OK");
                }
            };

        }

        async void StopScanning()
        {
            if (Adapter != null)
            {
                IsicDebug.DebugBluetooth(String.Format("Stopping scan."));
                await Adapter.StopScanningForDevicesAsync();
            }
        }


    }
}
