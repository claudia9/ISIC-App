using ISIC_FMT_MMCP_App;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;




namespace ISIC_FMT_MMCP_App
{

    public partial class DeviceTerminal : ContentPage
    {
        Guid WRITE_SERVICE = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
        Guid WRITE_CHARACTERISTIC = Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb");

        IAdapter currentAdapter;
        IDevice currentDevice;
        IService writeService;
        ICharacteristic writeCharacteristic;

        ObservableCollection<string> commandsHistoryList = new ObservableCollection<string>();

        public DeviceTerminal(IAdapter adapter, IDevice device)
        {
            //Automatically written to get the XAML
            InitializeComponent();

            currentAdapter = adapter;
            currentDevice = device;

            commandsHistoryList.Clear();
            CommandsHistory.ItemsSource = commandsHistoryList;

            Debug.WriteLine("State of the device: " + (string)device.Name + " is: " + device.State.ToString());

            //Click MAN command
            ManBtn.Clicked += (s, e) =>
            {
                findWriteCharacteristic(currentDevice);
                sendManCommand(writeCharacteristic);
            };

            Input.TextChanged += (s, e) =>
            {
                Char[] input = Input.Text.ToCharArray();
                Debug.WriteLine("String input = " + input);
            };

            SendCmd.Clicked += (s, e) =>
            {
                findWriteCharacteristic(currentDevice);
                sendInputCommand(Input.Text, writeCharacteristic);
            };

            Disconnect.Clicked += Disconnect_Clicked;
        }

        private void Disconnect_Clicked(object sender, EventArgs e)
        {
            currentAdapter.DisconnectDeviceAsync(currentDevice);
            Debug.WriteLine("State of the device: " + (string)currentDevice.Name + " is: " + currentDevice.State.ToString());

            Disconnect.Text = "Connect";
            Disconnect.Clicked -= Disconnect_Clicked;
            Disconnect.Clicked += Connect_Clicked;
        }

        private void Connect_Clicked(object arg1, EventArgs arg2)
        {
            currentAdapter.ConnectToDeviceAsync(currentDevice);
            Debug.WriteLine("State of the device: " + (string)currentDevice.Name + " is: " + currentDevice.State.ToString());
            Disconnect.Text = "Disconnect";
            Disconnect.Clicked -= Connect_Clicked;
            Disconnect.Clicked += Disconnect_Clicked;
        }

        private async void findWriteCharacteristic(IDevice currentDevice)
        {
            writeService = await currentDevice.GetServiceAsync(WRITE_SERVICE);
            System.Diagnostics.Debug.WriteLine("Write service found: " + writeService.ToString());

            writeCharacteristic = await writeService.GetCharacteristicAsync(WRITE_CHARACTERISTIC);
            System.Diagnostics.Debug.WriteLine("Write characteristic found: " + writeCharacteristic.ToString());
        }

        private void sendManCommand(ICharacteristic characteristic)
        {
            if (currentAdapter.ConnectedDevices.Contains(currentDevice))
            {
                try
                {
                    //MAN Command hardcoded
                    Byte[] bytes = { 0x07, 0x00, 0x4D, 0x41, 0x4E, 0x00, 0x1C };
                    characteristic.WriteAsync(bytes);

                    string command = DateTime.Now.ToString();
                    commandsHistoryList.Add(command + ": MAN");

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception - SendManCommand " + e.Message);
                }
            }
            else
            {
                DisplayAlert("Device not connected", "Your device is not connected, try to connect again before sending a command", "OK");
            }
        }


        private void sendInputCommand(string input, ICharacteristic characteristic)
        {
            if (input != null && currentAdapter.ConnectedDevices.Contains(currentDevice))
            {
                byte[] bytes = input.GetBytes();

                try
                {
                    characteristic.WriteAsync(bytes);

                    string command = DateTime.Now.ToString();
                    commandsHistoryList.Add(command + ": " + input);

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception - SendInputCommand " + e.Message);
                }
            } else
            {
                DisplayAlert("Device not connected", "Your device is not connected, try to connect again before sending a command", "OK");
            }
        }
    }

}