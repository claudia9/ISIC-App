using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using Xamarin.Forms;
using System;

namespace ISIC_FMT_MMCP_App
{
    public partial class DeviceList : ContentPage
    {
        //Collection of IDevice to collect each device encountered
        ObservableCollection<IDevice> devicesList = new ObservableCollection<IDevice>();


        //Bluetooth Settings
        IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        IDevice currentDevice;

        public DeviceList()
        {
            //Automatically written to get the XAML
            InitializeComponent();
            
            //Bluetooth Connection
            InitiliazeDefaultBluetooth();

            //Event handler for the Scan Button
            ScanAllButton.Clicked += (sender, e) =>
            {
                InitiliazeBluetooth();

            };
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
            try
            {
                currentDevice = await adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-f0c77f1c2065"));        //bleCACA
                //currentDevice = await adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-a81b6aaec165"));        //HELLOMISTER
                if (currentDevice != null)
                {

                    Debug.WriteLine("Connected to device: " + currentDevice.Name);
                    await Navigation.PushAsync(new RemoteControlPage(currentDevice));
                }
            } catch (Exception e)
            {
                Debug.WriteLine("Could not connect to default device!! Exception: " + e); 
            }
            
        }

        private void InitiliazeBluetooth()
        {
            //Initiliasing BluetoothLE and BleAdapter
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;

            Debug.WriteLine("BluetoothLE initiliased correctly.");

            ble.StateChanged += (s, e) =>
            {
                Debug.WriteLine("The bluetooth state changed to {e.NewState}");
            };

            //Delete any rests of the last Bluetooth scans
            devicesList.Clear();
            if(currentDevice != null)
            {
                Debug.WriteLine("Disconnecting from current device: " + currentDevice.Name);
                adapter.DisconnectDeviceAsync(currentDevice);
            }

            //Start scanning
            StartScanning(adapter);


            //Initiliase list of devices
            adapter.DeviceDiscovered += (s, a) =>
            {
                if (!devicesList.Contains(a.Device))
                {
                    devicesList.Add(a.Device);
                    Debug.WriteLine("Device found: " + a.Device.Name);
                    Debug.WriteLine("Device found: " + a.Device.Id);
                }
            };

            //Event handlers to show the List of devices once found
            listView.ItemsSource = devicesList;
            listView.ItemSelected += OnItemSelected;
        }

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem == null)
            {
                return;
            }

            StopScanning(CrossBluetoothLE.Current.Adapter);

            var connectedDevice = e.SelectedItem as IDevice;
            if (connectedDevice != null)
            {
                Debug.WriteLine("Selected device: " + connectedDevice.Name);
                try
                {
                    await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
                    Debug.WriteLine("Connected to device " + connectedDevice.Name);
                }
                catch (DeviceConnectionException ex)
                {
                    Debug.WriteLine("Could not connect to device: " + connectedDevice.Name);
                }

                ((ListView)sender).SelectedItem = null;

                await Navigation.PushAsync(new RemoteControlPage(connectedDevice));
            }
        }

        async void StartScanning(IAdapter adapter)
        {
            Debug.WriteLine(" ------- Start scanning -----------");
            devicesList.Clear();

            if (adapter.IsScanning)
            {
                await adapter.StopScanningForDevicesAsync();
                Debug.WriteLine("Adapter already trying to scan. STOPPING LAST SCAN and TRY AGAIN");
                await adapter.StartScanningForDevicesAsync();
            }
            else
            {
                await adapter.StartScanningForDevicesAsync();
                Debug.WriteLine("Cleared list of devices and starting scanning.");
            }
            adapter.ScanTimeoutElapsed += (s, e) =>
            {
                if (devicesList.Count() == 0)
                {
                    DisplayAlert("No devices found", "No devices have been found", "OK");
                }
                else
                {
                    DisplayAlert("Time out", "Time out has been reached. If the target device do not appear, try again", "OK");
                }
            };

        }

        void StopScanning(IAdapter adapter)
        {
            if (adapter.IsScanning)
            {
                adapter.StopScanningForDevicesAsync();
                Debug.WriteLine("Still scanning, stopping the scan");
            }
        }


    }
}
