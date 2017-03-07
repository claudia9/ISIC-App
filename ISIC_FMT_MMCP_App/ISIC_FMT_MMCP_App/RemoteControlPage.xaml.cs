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

        IDevice currentDevice;
        ICharacteristic currentCharacteristic;

        public RemoteControlPage(IDevice device)
        {
            Debug.WriteLine("Initiliasing Remote Control Page with Device: " + device.Name);

            InitilizeScreen();             //Hide Navigation Bar
            InitializeComponent();          //Create visual interface
            InitializeBluetooth(device);
            InitializeDictionary();         //Create object of MonitorSettings for each monitor
            InitiliazeMonitor();            //Create current monitor
            InitializeButtons();            //Create event handlers for each button

            Debug.WriteLine("Finished initiliazations");
        }

        #region Initializers
        private void InitilizeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void InitializeBluetooth(IDevice device)
        {
            currentDevice = device;
            findWriteCharacteristic(currentDevice);
        }

        private async void findWriteCharacteristic(IDevice currentDevice)
        {
            Guid WRITE_SERVICE = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
            Guid WRITE_CHARACTERISTIC = Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb");

            var service = await currentDevice.GetServiceAsync(WRITE_SERVICE);
            Debug.WriteLine("Write service found: " + service.ToString());

            currentCharacteristic = await service.GetCharacteristicAsync(WRITE_CHARACTERISTIC);
            Debug.WriteLine("Write characteristic found: " + currentCharacteristic.ToString());
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

        private void InitiliazeMonitor()
        {
            currentMonitor = monitors[MonitorIdentifier.Monitor1];
            Debug.WriteLine("- INITIALIZE MONITOR: currentMonitor.MonAddr = " + currentMonitor.MonAddr + "currentMonitor.ToD = " + currentMonitor.ToD + " currentMonitor.Backlight = " + currentMonitor.ToDBacklightValue);
        }

        private void InitializeButtons()
        {
            Monitor1.Clicked += Monitor1_Clicked;
            Monitor2.Clicked += Monitor2_Clicked;
            Monitor3.Clicked += Monitor3_Clicked;
            MonitorAll.Clicked += MonitorAll_Clicked;

            NightMode.Clicked += NightMode_Clicked;
            DuskMode.Clicked += DuskMode_Clicked;
            DayMode.Clicked += DayMode_Clicked;

            VGA.Clicked += VGA_Clicked;
            DVI.Clicked += DVI_Clicked;
            DP.Clicked += DP_Clicked;

            Slider.ValueChanged += Slider_ValueChanged;

            SettingsButton.Clicked += SettingsButton_Clicked;
            ScanButton.Clicked += ScanAllButton_Clicked;
        }

        #endregion Initializers

        #region SetAddresses
        private void setMonitor1Address(Byte address)
        {
            SetMonitorAddress(address, MonitorIdentifier.Monitor1);
        }
        private void setMonitor2Address(Byte address)
        {
            SetMonitorAddress(address, MonitorIdentifier.Monitor2);
        }
        private void setMonitor3Address(Byte address)
        {
            SetMonitorAddress(address, MonitorIdentifier.Monitor3);
        }
        private void setMonitor4Address(Byte address)
        {
            SetMonitorAddress(address, MonitorIdentifier.Monitor4);
        }

        private void SetMonitorAddress(Byte address, MonitorIdentifier monIdentifier)
        {
            monitors[monIdentifier].MonAddr = address;
        }
        #endregion

        #region Monitor Clicks

        private void Monitor1_Clicked(object sender, EventArgs e)
        {
            SetMonitor(MonitorIdentifier.Monitor1);
        }

        private void Monitor2_Clicked(object sender, EventArgs e)
        {
            SetMonitor(MonitorIdentifier.Monitor2);
        }
        private void Monitor3_Clicked(object sender, EventArgs e)
        {
            SetMonitor(MonitorIdentifier.Monitor3);
        }

        private void MonitorAll_Clicked(object sender, EventArgs e)
        {
            SetMonitor(MonitorIdentifier.MonitorBroadcast);
        }



        private void SetMonitor(MonitorIdentifier monIdentifier)
        {
            currentMonitor = monitors[monIdentifier];
        }
        #endregion

        #region Slider
        bool isSending;

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!isSending)
            {
                isSending = true;
                Byte value = (Byte)(sender as Slider).Value;
                new Isic.SerialProtocol.Command(currentMonitor.MonAddr, ISIC_SCP_IF.CMD_BRT, value).Send(currentCharacteristic);
                //String c = value.ToString("X2");
                //byte[] bytes = { 0x07, 0x01, 0x4D, 0x43, 0x43, 0x03, 0x21, 0x59, (Byte)c[0], (Byte)c[1], 0x46 };
                //await characteristic.WriteAsync(bytes);
                isSending = false;
            }

        }
        #endregion

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
                new Isic.SerialProtocol.Command(currentMonitor.MonAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_MPC, (byte)inputValue.ToString("X2")[0], (byte)inputValue.ToString("X2")[1]).Send(currentCharacteristic);
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
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_DAY);
        }

        private void DuskMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Dusk Command");
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_DUSK);
        }

        private void NightMode_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send Night Command");
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_NIGHT);
        }
        private async void SetMonitorSettings(Byte mode)
        {
            Debug.WriteLine("Setting Monitor Settings");
            if (currentMonitor != null)
            {
                if (currentMonitor.MonAddr == monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
                {
                    foreach (var item in monitors)
                    {
                        item.Value.ToD = mode;
                    }
                }
                else
                {
                    Debug.WriteLine("Setting single monitor -> MonAddr: " + currentMonitor.MonAddr);
                    /*if (!await isMonitorAvailable(currentMonitor.MonAddr))
                    {
                        Debug.WriteLine("currentMonitor not replying!");
                        return;
                    }*/
                    Debug.WriteLine("Set currentMonitor to mode: " + mode);
                    currentMonitor.ToD = mode;
                }
            }
            SendModeData(mode);
            if (currentMonitor.MonAddr != monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
            {
                await Task.Delay(100);
                //queryBacklight();
            }
        }

        private void SendModeData(Byte mode)
        {
            try
            {
                Debug.WriteLine("- SENDING MODE DATA: Command: " + ISIC_SCP_IF.CMD_ECD + ", Mon addr: " + currentMonitor.MonAddr + ", Mode: " + mode.ToString());
                new Isic.SerialProtocol.Command(currentMonitor.MonAddr, ISIC_SCP_IF.CMD_ECD, mode).Send(currentCharacteristic);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error trying to send command" + e.Message);
            }
        }

        #endregion  Mode Clicks

        #region Check Availability Monitors
        private async Task<bool> isMonitorAvailable(byte monAddr)
        {
            if (currentCharacteristic.CanRead)
            {
                Debug.WriteLine("Sending data to monitor to make sure isMonitorAvailable?");
                try
                {
                    Byte[] rArr;
                    new Isic.SerialProtocol.Command(monAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_BKL, 0x3F).Send(currentCharacteristic);
                    rArr = await currentCharacteristic.ReadAsync();
                    if (rArr == null || rArr.Length == 0)
                    {
                        return false;
                    }
                    Debug.WriteLine("Received data from monitor: " + rArr.GetHexString());
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not send query to the monitor. Ex -> " + ex.Message);
                    return false;
                }

            }
            Debug.WriteLine("Characteristic could not read, returning false");
            return false;
        }
        #endregion


        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new  MonitorSettingsPage(), true);
        }


        private void ScanAllButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnBackButtonPressed");
            if (currentDevice != null)
            {
                Debug.WriteLine("OnSettingsClicked - currentDevice: " + currentDevice.Name + " is not null");

                IAdapter adapter = CrossBluetoothLE.Current.Adapter;


                for (int i = 0; i < adapter.ConnectedDevices.Count(); i++)
                {
                    adapter.DisconnectDeviceAsync(adapter.ConnectedDevices[i]);
                    adapter.ConnectedDevices.Remove(adapter.ConnectedDevices[i]);
                }
            }

            Debug.WriteLine("OnSettingsClicked - currentDevice is null");
            Navigation.PopAsync(true);
        }

    }
}
