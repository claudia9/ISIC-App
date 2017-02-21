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
        }

        private async void findWriteCharacteristics(IDevice connectedDevice)
        {
            try
            {
                var service = await connectedDevice.GetServiceAsync(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));
                System.Diagnostics.Debug.WriteLine("Unknown fucking service found: " + service.ToString());

                var characteristic = await service.GetCharacteristicAsync(Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb"));
                System.Diagnostics.Debug.WriteLine("Unknown fucking characteristic found: " + characteristic.ToString());

                Byte[] bytes = { 0x43, 0x4c, 0x41, 0x55, 0x44, 0x49, 0x41 };
                await characteristic.WriteAsync(bytes);

            }
            catch (Exception e)
            {
                Debug.WriteLine("DiscoverServices() " + e.Message);
            }
        }
    }
}
