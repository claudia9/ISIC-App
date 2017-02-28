using Isic.SerialProtocol;
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
    public enum MonitorIdentifier
    {
        Monitor1,
        Monitor2,
        Monitor3,
        Monitor4,
        MonitorBroadcast
    }

    public partial class RemoteControlPage : ContentPage
    {
        private Dictionary<MonitorIdentifier, MonitorSettings> monitors { get; set; }
        private MonitorSettings currentMonitor = null;

        IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        IDevice currentDevice;
        IService service;
        ICharacteristic characteristic;
        public RemoteControlPage(IDevice device)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            InitializeDictionary();

            InitializeButtons();

            InitiliazeBluetooth(device);

        }

        private void InitializeDictionary()
        {
            monitors = new Dictionary<MonitorIdentifier, MonitorSettings>();
            monitors[MonitorIdentifier.Monitor1] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor2] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor3] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor4] = new MonitorSettings();
            monitors[MonitorIdentifier.MonitorBroadcast] = new MonitorSettings() { MonAddr = 0xFF } ;
        }

        private async void InitiliazeBluetooth(IDevice device)
        {
            //Initiliaze BluetoothLE Known Device
            if (currentDevice == null)
            {
                Debug.WriteLine("Connecting to device - Hardcoded.");
                currentDevice = await adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-f0c77f1c2065"));
            } else
            {

                Debug.WriteLine("Connecting to device - From list.");
                currentDevice = device;
            }
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
                String c = value.ToString("X2");
                byte[] bytes = { 0x07, 0x01, 0x4D, 0x43, 0x43, 0x03, 0x21, 0x59, (Byte)c[0], (Byte)c[1], 0x46 };
                //byte[] bytes = new byte[1];
                //bytes[0] = b;
                await characteristic.WriteAsync(bytes);
                isSending = false;
            }

        }

        #region Input Clicks
        private void DP_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set DP Command");
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DP);
        }

        private void DVI_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set DVI Command");
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DVI);
        }

        private void VGA_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Set VGA Command");
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_VGA);
        }

        private void sendInputData(Byte inputValue)
        {
            try
            {
                new Isic.SerialProtocol.Command(currentMonitor.MonAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_MPC, (byte)inputValue.ToString("X2")[0], (byte)inputValue.ToString("X2")[1]).Send(characteristic);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error trying to send command" + e.Message);
            }
        }
        #endregion Input Clicks

        #region Mode Clicks
        private void DayMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Day Command");
            SendModeData(ISIC_SCP_IF.BYTE_DATA_ECD_DAY);
        }

        private void DuskMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Dusk Command");
            SendModeData(ISIC_SCP_IF.BYTE_DATA_ECD_DUSK);
        }

        private void NightMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Night Command");
            SendModeData(ISIC_SCP_IF.BYTE_DATA_ECD_NIGHT);
        }

        private void SendModeData(Byte  mode)
        {
            try
            {

                new Isic.SerialProtocol.Command(currentMonitor.MonAddr, ISIC_SCP_IF.CMD_ECD, mode).Send(characteristic);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error trying to send command" + e.Message);
            }
        }

        #endregion  Mode Clicks
       /* private void setMonitorSettings(Byte mode)
        {
            if (currentMonitor != null)
            {
                if (currentMonitor.MonAddr = monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
                {
                    foreach (var item in monitors)
                    {
                        item.Value.ToD = mode;
                    }
                } else
                {
                    if (!isMonitorAvailable(currentMonitor.MonAddr))
                    {
                        return;
                    }
                    currentMonitor.ToD = mode;
                }
            }
            SendModeData(mode);
            if (currentMonitor.MonAddr != monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
            {
                Task.Delay(100);
                queryBacklight();
            }
        }*/


        void OnSettingsClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnSettingsClicked");
            if (currentDevice != null)
            {
                Debug.WriteLine("OnSettingsClicked - currentDevice: " + currentDevice.Name + " is not null");
                adapter.DisconnectDeviceAsync(currentDevice);
            }

            Debug.WriteLine("OnSettingsClicked - currentDevice is null");
            Navigation.PushAsync(new DeviceList());
        }

    }
}
