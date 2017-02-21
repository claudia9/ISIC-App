using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class DeviceList : ContentPage
    {
        //Collection of IDevice to collect each device encountered
        ObservableCollection<IDevice> devicesList = new ObservableCollection<IDevice>();

        public DeviceList()
        {
            //Automatically written to get the XAML
            InitializeComponent();

            //Bluetooth Connection
            InitiliazeBluetooth();

            //Event handler for the Scan Button
            ScanAllButton.Clicked += (sender, e) =>
            {
                InitiliazeBluetooth();

            };


        }

        private async void InitiliazeBluetooth()
        {
            //Initiliasing BluetoothLE and BleAdapter
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;

            System.Diagnostics.Debug.WriteLine("BluetoothLE initiliased correctly.");

            ble.StateChanged += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("The bluetooth state changed to {e.NewState}");
            };

            //Delete any rests of the last Bluetooth scans
            devicesList.Clear();

            //Start scanning
            StartScanning(adapter);
            await adapter.StartScanningForDevicesAsync();

            //Initiliase list of devices
            adapter.DeviceDiscovered += (s, a) =>
            {
                devicesList.Add(a.Device);
                System.Diagnostics.Debug.WriteLine("Device found: " + a.Device.Name);
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
                System.Diagnostics.Debug.WriteLine("Selected device: " + connectedDevice.Name);
                try
                {
                    await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
                    System.Diagnostics.Debug.WriteLine("Connected to device " + connectedDevice.Name);
                }
                catch (DeviceConnectionException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not connect to device: " + connectedDevice.Name);
                }

                ((ListView)sender).SelectedItem = null;

                await Navigation.PushAsync(new DeviceTerminal(CrossBluetoothLE.Current.Adapter, connectedDevice));
            }
        }

        void StartScanning(IAdapter adapter)
        {
            System.Diagnostics.Debug.WriteLine(" ------- Start scanning -----------");
            if (adapter.IsScanning)
            {
                adapter.StopScanningForDevicesAsync();
                System.Diagnostics.Debug.WriteLine("Adapter already trying to scan. STOPPING SCAN");
            }
            else
            {
                devicesList.Clear();
                adapter.StartScanningForDevicesAsync();
                System.Diagnostics.Debug.WriteLine("Cleared list of devices and starting scanning.");
            }
        }

        void StopScanning(IAdapter adapter)
        {
            if (adapter.IsScanning)
            {
                adapter.StopScanningForDevicesAsync();
                System.Diagnostics.Debug.WriteLine("Still scanning, stopping the scan");
            }
        }
    }
}
