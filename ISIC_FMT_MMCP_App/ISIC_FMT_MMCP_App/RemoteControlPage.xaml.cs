using Acr.UserDialogs;
using Isic.Debugger;
using Isic.SerialProtocol;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{


    public partial class RemoteControlPage : ContentPage
    {
        private Dictionary<MonitorIdentifier, MonitorSettings> Monitors { get; set; }
        private MonitorSettings CurrentMonitor = null;

        IDevice CurrentDevice;
        ICharacteristic CurrentCharacteristic;

        public RemoteControlPage(IDevice device)
        {
            CurrentDevice = device;
            IsicDebug.DebugGeneral(String.Format("Initiliasing Remote Control Page with Device: {0}", CurrentDevice.Name));

            InitilizeScreen();             //Hide Navigation Bar
            InitializeComponent();          //Create visual interface
            InitializeBluetooth();          //Check characteristics of the BluetoothLe Device.
            InitializeDictionary();         //Create object of MonitorSettings for each monitor
            InitializeButtons();            //Create event handlers for each button

            InitiliazeMonitor();            //Create current monitor
            IsicDebug.DebugGeneral(String.Format("Finished initiliazations"));
        }


        #region Initializers
        private void InitilizeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void InitializeBluetooth()
        {
            findWriteCharacteristic();
        }

        private async void findWriteCharacteristic()
        {
            if (CurrentDevice != null)
            {
                try
                {
                    Guid WRITE_SERVICE = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
                    Guid WRITE_CHARACTERISTIC = Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb");

                    var Service = await CurrentDevice.GetServiceAsync(WRITE_SERVICE);
                    if (Service != null) IsicDebug.DebugBluetooth(String.Format("Write service found: {0}", Service.Name));

                    CurrentCharacteristic = await Service.GetCharacteristicAsync(WRITE_CHARACTERISTIC);
                    IsicDebug.DebugBluetooth(String.Format("Write characteristic found: {0}", CurrentCharacteristic.Name));

                } catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("Could not find Characteristics. {0}", ex));
                    UserDialogs.Instance.Alert("This Bluetooth device does not allow to send Serial Data, please, choose another Bluetooth device of the list", null, "Ok");
                    await Task.Delay(1000);
                    await Navigation.PopToRootAsync();
                }
            }

        }

        private void InitializeDictionary()
        {
            Monitors = new Dictionary<MonitorIdentifier, MonitorSettings>();
            Monitors[MonitorIdentifier.Monitor1] = new MonitorSettings();
            Monitors[MonitorIdentifier.Monitor2] = new MonitorSettings();
            Monitors[MonitorIdentifier.Monitor3] = new MonitorSettings();
            Monitors[MonitorIdentifier.MonitorBroadcast] = new MonitorSettings() { MonAddr = 0xFF };
        }

        private void InitiliazeMonitor()
        {
            if (CurrentMonitor == null)
            {
                DisableButtons();
                UserDialogs.Instance.Toast("Choose one monitor before proceding. (Press ALL to send broadcast commands", TimeSpan.FromSeconds(2.5));
            }
        }

        private void DisableButtons()
        {
            NightMode.IsEnabled = false;
            DuskMode.IsEnabled = false;
            DayMode.IsEnabled = false;
            VGA.IsEnabled = false;
            DVI.IsEnabled = false;
            DP.IsEnabled = false;
            Slider.IsEnabled = false;
        }
        private void EnableButtons()
        {
            NightMode.IsEnabled = true;
            DuskMode.IsEnabled = true;
            DayMode.IsEnabled = true;
            VGA.IsEnabled = true;
            DVI.IsEnabled = true;
            DP.IsEnabled = true;
            Slider.IsEnabled = true;
            NightMode.TextColor = Color.White;
            DuskMode.TextColor = Color.White;
            DayMode.TextColor = Color.White;
            VGA.TextColor = Color.White;
            DVI.TextColor = Color.White;
            DP.TextColor = Color.White;
            Slider.IsEnabled = true;
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
            ScanButton.Clicked += ScanButton_Clicked;
        }

        #endregion Initializers
        
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

        private void SetMonitor(MonitorIdentifier MonIdentifier)
        {
            EnableButtons();

            switch (MonIdentifier)
            {
                case MonitorIdentifier.Monitor1:
                    Monitors[MonIdentifier].MonAddr = Convert.ToByte(Application.Current.Properties["Mon1Addr"]);
                    Monitor1.TextColor = Color.FromHex("#64B22E");
                    Monitor1.FontAttributes = FontAttributes.Bold;
                    Monitor2.TextColor = Color.FromHex("#FFFFFF");
                    Monitor3.TextColor = Color.FromHex("#FFFFFF");
                    MonitorAll.TextColor = Color.FromHex("#FFFFFF");
                    break;
                case MonitorIdentifier.Monitor2:
                    Monitors[MonIdentifier].MonAddr = Convert.ToByte(Application.Current.Properties["Mon2Addr"]);
                    Monitor2.TextColor = Color.FromHex("#64B22E");
                    Monitor2.FontAttributes = FontAttributes.Bold;
                    Monitor1.TextColor = Color.FromHex("#FFFFFF");
                    Monitor3.TextColor = Color.FromHex("#FFFFFF");
                    MonitorAll.TextColor = Color.FromHex("#FFFFFF");
                    break;
                case MonitorIdentifier.Monitor3:
                    Monitors[MonIdentifier].MonAddr = Convert.ToByte(Application.Current.Properties["Mon3Addr"]);
                    Monitor3.TextColor = Color.FromHex("#64B22E");
                    Monitor3.FontAttributes = FontAttributes.Bold;
                    Monitor1.TextColor = Color.FromHex("#FFFFFF");
                    Monitor2.TextColor = Color.FromHex("#FFFFFF");
                    MonitorAll.TextColor = Color.FromHex("#FFFFFF");
                    break;
                case MonitorIdentifier.MonitorBroadcast:
                    MonitorAll.TextColor = Color.FromHex("#64B22E");
                    MonitorAll.FontAttributes = FontAttributes.Bold;
                    Monitor1.TextColor = Color.FromHex("#FFFFFF");
                    Monitor2.TextColor = Color.FromHex("#FFFFFF");
                    Monitor3.TextColor = Color.FromHex("#FFFFFF");
                    break;
            }

            CurrentMonitor = Monitors[MonIdentifier];
            IsicDebug.DebugMonitor(String.Format("CurrentMonitor initiliased: Address: {0}", CurrentMonitor.MonAddr));
            /*if (monIdentifier == MonitorIdentifier.MonitorBroadcast)
            {
                availableButtons();
            }
            else if (await isMonitorAvailable(currentMonitor.MonAddr))
            {
                availableButtons();
                setModeButtons();
                QueryBacklight();
                await queryInput();
            }
            else
            {
                monitorNotAvailable();
            }*/
        }
        #endregion

        #region Query info from monitor - NOT IN USE
        //Not in use
        private async void QueryBacklight()
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                byte[] rArr;
                int sliderDecValue;
                Stopwatch sw = new Stopwatch();
                sw.Start();

                do
                {
                    new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_BKL, 0x3F).Send(CurrentCharacteristic);
                    rArr = await CurrentCharacteristic.ReadAsync();


                    if (sw.ElapsedMilliseconds > 7000)
                    {
                        IsicDebug.DebugSerial(String.Format("Backlight query time out"));
                        //throw new TimeoutException();
                    }

                } while (rArr == null || rArr.Length == 0);

                sw.Reset();

                Slider.ValueChanged -= Slider_ValueChanged;

                sliderDecValue = Convert.ToInt32(rArr.GetHexString().Substring(ISIC_SCP_IF.BYTE_INDEX_IHCHK + 3, 2), 16);
                IsicDebug.DebugSerial(String.Format("Transformed backlight Data to value: {0}(dec)", sliderDecValue));
                CurrentMonitor.ToDBacklightValue = sliderDecValue;
                Slider.Value = sliderDecValue;
                Slider.ValueChanged += Slider_ValueChanged;
            } catch (Exception ex)
            {
                IsicDebug.DebugException(String.Format("Not able to send or receive the Backlight data", ex));
            }

        }

        //Not in use
        private async Task<bool> QueryInput()
        {
            int currentInput;
            try
            {
                new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_MPC, 0x3F).Send(CurrentCharacteristic);
                byte[] rArr = await CurrentCharacteristic.ReadAsync();

                if (rArr != null)
                {
                    IsicDebug.DebugSerial(String.Format("Received input Data: {0}", rArr.GetHexString()));
                    currentInput = Convert.ToInt32(rArr.GetString().Substring(ISIC_SCP_IF.BYTE_INDEX_IHCHK + 3, 2), 16);
                    IsicDebug.DebugSerial(String.Format("Transpormed input data to value: {0}", currentInput));
                    return true;
                } else
                {
                    IsicDebug.DebugSerial(String.Format("Not receiving any Input data from the monitor"));
                    return false;
                }
            } catch (Exception ex)
            {
                IsicDebug.DebugException(String.Format("Not able to send or receive input data.", ex));
                return false;
            }
        }

        //Not in use
        private void SetModeButtons()
        {
            if (CurrentMonitor != null)
            {
                if (CurrentMonitor.ToD == ISIC_SCP_IF.BYTE_DATA_ECD_DAY)
                {
                    IsicDebug.DebugMonitor(String.Format("currentMonitor.ToD = Day"));
                    DayMode.TextColor = Color.FromHex("#64B22E");
                    DuskMode.TextColor = Color.FromHex("#FFFFFF");
                    NightMode.TextColor = Color.FromHex("#FFFFFF");
                } else if(CurrentMonitor.ToD == ISIC_SCP_IF.BYTE_DATA_ECD_DUSK) {
                    IsicDebug.DebugMonitor(String.Format("currentMonitor.ToD = Dusk"));
                    DuskMode.TextColor = Color.FromHex("#64B22E");
                    DayMode.TextColor = Color.FromHex("#FFFFFF");
                    NightMode.TextColor = Color.FromHex("#FFFFFF");
                } else if (CurrentMonitor.ToD == ISIC_SCP_IF.BYTE_DATA_ECD_NIGHT) {
                    IsicDebug.DebugMonitor(String.Format("currentMonitor.ToD = NIght"));
                    NightMode.TextColor = Color.FromHex("#64B22E");
                    DayMode.TextColor = Color.FromHex("#FFFFFF");
                    DuskMode.TextColor = Color.FromHex("#FFFFFF");
                }
            }
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
                new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, ISIC_SCP_IF.CMD_BRT, value).Send(CurrentCharacteristic);
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
            IsicDebug.DebugSerial(String.Format("Set DP Command"));
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DP);
        }

        private void DVI_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugSerial(String.Format("Set DVI Command"));
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DVI);
        }

        private void VGA_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugSerial(String.Format("Set VGA Command"));
            sendInputData(ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_VGA);
        }

        private void sendInputData(Byte inputValue)
        {
            try
            {
                new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_MPC, (byte)inputValue.ToString("X2")[0], (byte)inputValue.ToString("X2")[1]).Send(CurrentCharacteristic);
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Error trying to send command. {0}", e.Message));
            }
        }
        #endregion Input Clicks

        #region Mode Clicks
        private void DayMode_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugSerial(String.Format("Send Day Command"));
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_DAY);
        }

        private void DuskMode_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugSerial(String.Format("Send Dusk Command"));
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_DUSK);
        }

        private void NightMode_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugSerial(String.Format("Send Night Command"));
            SetMonitorSettings(ISIC_SCP_IF.BYTE_DATA_ECD_NIGHT);
        }
        private async void SetMonitorSettings(Byte mode)
        {
            IsicDebug.DebugMonitor(String.Format("Setting Monitor Settings"));
            if (CurrentMonitor != null)
            {
                if (CurrentMonitor.MonAddr == Monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
                {
                    foreach (var item in Monitors)
                    {
                        item.Value.ToD = mode;
                    }
                }
                else
                {
                    IsicDebug.DebugMonitor(String.Format("Setting single monitor -> MonAddr: {0}", CurrentMonitor.MonAddr));
                    /*if (!await isMonitorAvailable(currentMonitor.MonAddr))
                    {
                        Debug.WriteLine("currentMonitor not replying!");
                        return;
                    }*/
                    IsicDebug.DebugMonitor(String.Format("Set currentMonitor to mode: {0}", mode));
                    CurrentMonitor.ToD = mode;
                }
            }
            SendModeData(mode);
            if (CurrentMonitor.MonAddr != Monitors[MonitorIdentifier.MonitorBroadcast].MonAddr)
            {
                await Task.Delay(100);
                //queryBacklight();
            }
        }

        private void SendModeData(Byte mode)
        {
            try
            {
                IsicDebug.DebugMonitor(String.Format("Sending mode data: Command: {0}, MonAddr: {1}, Mode: {2}", ISIC_SCP_IF.CMD_ECD, CurrentMonitor.MonAddr, mode.ToString()));
                new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, ISIC_SCP_IF.CMD_ECD, mode).Send(CurrentCharacteristic);
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Error trying to send command. {0}", e.Message));
            }
        }

        #endregion  Mode Clicks

        #region Check Availability Monitors - NOT IN USE
        private async Task<bool> isMonitorAvailable(byte monAddr)
        {
            if (CurrentCharacteristic.CanRead)
            {
                IsicDebug.DebugSerial(String.Format("Sending data to monitor to make sure isMonitorAvailable?"));
                try
                {
                    Byte[] rArr;
                    new Isic.SerialProtocol.Command(monAddr, ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_BKL, 0x3F).Send(CurrentCharacteristic);
                    rArr = await CurrentCharacteristic.ReadAsync();
                    if (rArr == null || rArr.Length == 0)
                    {
                        return false;
                    }
                    IsicDebug.DebugMonitor(String.Format("Received data from monitor: {0}", rArr.GetHexString()));
                    return true;
                }
                catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("Could not send query to the monitor. {0}", ex.Message));
                    return false;
                }

            }
            IsicDebug.DebugSerial(String.Format("No data received from monitor, returning false"));
            return false;
        }
        #endregion


        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new  MonitorSettingsPage(), true);
        }


        private void ScanButton_Clicked(object sender, EventArgs e)
        {
            IsicDebug.DebugGeneral(String.Format("Scan button clicked"));

            IAdapter adapter = CrossBluetoothLE.Current.Adapter;

            if (adapter != null)
            {
                for (int i = 0; i < adapter.ConnectedDevices.Count(); i++)
                {
                    adapter.DisconnectDeviceAsync(adapter.ConnectedDevices[i]);
                }
                IsicDebug.DebugBluetooth(String.Format("Adapter not connected to any device. Number of connected devices is: {0}", adapter.ConnectedDevices.Count));
            }

            Navigation.PopToRootAsync(true);
        }

    }
}
