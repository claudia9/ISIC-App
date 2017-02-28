using Plugin.BLE;
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
    public partial class RemoteControlPage : ContentPage
    {
        IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        IDevice currentDevice;
        IService service;
        ICharacteristic characteristic;
        public RemoteControlPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            InitializeButtons();

            InitiliazeBluetooth();
            
        }

        private async void InitiliazeBluetooth()
        {   
            //Initiliaze BluetoothLE Known Device
            currentDevice = await adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-f0c77f1c2065"));
            Debug.WriteLine("Connected to device: " + currentDevice.Name);

            //Initialize BluetoothLE Write Characteristic
            findWriteCharacteristic(currentDevice);
        }

        private async void findWriteCharacteristic(IDevice currentDevice)
        {
            Guid WRITE_SERVICE = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
            Guid WRITE_CHARACTERISTIC = Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb");

            service = await currentDevice.GetServiceAsync(WRITE_SERVICE);
            Debug.WriteLine("Write service found: " + service.ToString());

            characteristic = await service.GetCharacteristicAsync(WRITE_CHARACTERISTIC);
            Debug.WriteLine("Write characteristic found: " + characteristic.ToString());
        }

        private void InitializeButtons()
        {
            NightMode.Clicked += NightMode_Clicked;
            DuskMode.Clicked += DuskMode_Clicked;
            DayMode.Clicked += DayMode_Clicked;

            VGA.Clicked += VGA_Clicked;
            DVI.Clicked += DVI_Clicked;
            DP.Clicked += DP_Clicked;

            Slider.ValueChanged += Slider_ValueChanged;
        }

        bool isSending;

        private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!isSending)
            {
                isSending = true;
                Byte value = (Byte)(sender as Slider).Value;
                //Byte a = (byte)((value >> 4) & 0x0F).ToString("X2")[0];
                //Byte b = (byte)((value) & 0x0F).ToString("X2")[0];
                String c = value.ToString("X2");
                byte[] bytes = { 0x07, 0x01, 0x4D, 0x43, 0x43, 0x03, 0x21, 0x59, (Byte)c[0], (Byte)c[1], 0x46 };
                //byte[] bytes = new byte[1];
                //bytes[0] = b;
                await characteristic.WriteAsync(bytes);
                isSending = false;
            }

        }

        private void DP_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set DP Command");
            byte[] bytes = { 0x07, 0x00, 0x4D, 0x43, 0x43, 0x03, 0x22, 0x98, 0x30, 0x32, 0x05 };
            sendInputData(bytes);
            
        }

        private void DVI_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set DVI Command");
            byte[] bytes = { 0x07, 0x00, 0x4D, 0x43, 0x43, 0x03, 0x22, 0x98, 0x30, 0x31, 0x06 };
            sendInputData(bytes);
        }

        private void VGA_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set VGA Command");
            byte[] bytes = { 0x07, 0x00, 0x4D, 0x43, 0x43, 0x03, 0x22, 0x98, 0x30, 0x30, 0x07 };
            sendInputData(bytes);
        }

        private void sendInputData(byte[] bytes)
        {
            try
            {
                //Write bytes through the Write Characteristic
                characteristic.WriteAsync(bytes);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error trying to send command" + e.Message);
            }
        }

        private void DayMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Day Command");
            byte[] bytes = { 0x07, 0xFF, 0x45, 0x43, 0x44, 0x01, 0x2C, 0x00, 0xFF }; 
            //byte[] bytes = { 0x07, 0x00, 0x45, 0x43, 0x44, 0x01, 0x2B, 0x00, 0xFF };
            characteristic.WriteAsync(bytes);
        }

        private void DuskMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Dusk Command");
            byte[] bytes = { 0x07, 0xFF, 0x45, 0x43, 0x44, 0x01, 0x2C, 0x01, 0xFE };
            //byte[] bytes = { 0x07, 0x00, 0x45, 0x43, 0x44, 0x01, 0x2B, 0x01, 0xFE };
            characteristic.WriteAsync(bytes);
        }

        private void NightMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Night Command");
            byte[] bytes = { 0x07, 0xFF, 0x45, 0x43, 0x44, 0x01, 0x2C, 0x02, 0xFD };
            //byte[] bytes = { 0x07, 0x00, 0x45, 0x43, 0x44, 0x01, 0x2B, 0x02, 0xFD };
            characteristic.WriteAsync(bytes);
        }

        void OnSettingsClicked(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                adapter.DisconnectDeviceAsync(currentDevice);
            }
            Navigation.PushAsync(new DeviceList());
        }

    }
    public class ISIC_SCP_IF
    {
        byte[] BYTE_DATA_MMC_VALUE_MPC_DP = { 0x07, 0x00, 0x4D, 0x43, 0x43, 0x03, 0x22, 0x98, 0x30, 0x32, 0x05 };
    }
}
