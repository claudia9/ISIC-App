using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class DeviceTerminal : ContentPage
    {
        public DeviceTerminal(IAdapter adapter, IDevice device)
        {
            //Automatically written to get the XAML
            InitializeComponent();

            IAdapter currentAdapter = adapter;
            IDevice currentDevice = device;

            Debug.WriteLine("State of the device: " + (string)device.Name + " is: " + device.State.ToString());

            //Click MAN command
            ManBtn.Clicked += (s, e) =>
            {
                findWriteCharacteristics(currentDevice);
            };

            SendCmd.Clicked += (s, e) =>
            {

            };

            Disconnect.Clicked += (s, e) =>
            {
                adapter.DisconnectDeviceAsync(currentDevice);
                Disconnect.Text = "Connect";
            };
        }

        private async void findWriteCharacteristics(IDevice connectedDevice)
        {
            try
            {
                var service = await connectedDevice.GetServiceAsync(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));
                System.Diagnostics.Debug.WriteLine("Unknown fucking service found: " + service.ToString());

                var characteristic = await service.GetCharacteristicAsync(Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb"));
                System.Diagnostics.Debug.WriteLine("Unknown fucking characteristic found: " + characteristic.ToString());

                //MAN Command hardcoded
                Byte[] bytes = { 0x07, 0x00, 0x4D, 0x41, 0x4E, 0x00, 0x1C };
                await characteristic.WriteAsync(bytes);

            }
            catch (Exception e)
            {
                Debug.WriteLine("DiscoverServices() " + e.Message);
            }
        }

    }
}
