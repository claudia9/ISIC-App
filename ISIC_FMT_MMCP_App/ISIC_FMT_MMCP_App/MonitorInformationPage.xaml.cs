using Acr.UserDialogs;
using Isic.Debugger;
using Isic.SerialProtocol;
using Isic.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorInformationPage : ContentPage
    {
        private MonitorSettings CurrentMonitor;
        ICharacteristic CurrentCharacteristic;

        LoadingViewModel loadingViewModel;

        public MonitorInformationPage(MonitorSettings monitor, ICharacteristic characteristic)
        {
            this.CurrentMonitor = monitor;
            this.CurrentCharacteristic = characteristic;
            this.loadingViewModel = new LoadingViewModel(UserDialogs.Instance);


            if (CurrentMonitor != null)
            {
                IsicDebug.DebugGeneral(String.Format("Initiliazed Monitor Information Page with Monitor at address: {0}", CurrentMonitor.MonAddr));
            }

            InitializeScreen();
            InitializeComponent();
            InitializeButtons();
            InitializeMonitorInfo();
        }

        private void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void InitializeButtons()
        {
            BackButton.Clicked += BackButton_Clicked;

            Buzzer.Toggled += Buzzer_Toggled;

            Fan.Toggled += Fan_Toggled;
        }


        private async void InitializeMonitorInfo()
        {
            Title.Text = "Monitor at address: " + CurrentMonitor.MonAddr.ToString();
            UserDialogs.Instance.ShowLoading("Retrieving data - This can take some seconds", MaskType.Black);
            await getMonitorInfo();
            UserDialogs.Instance.HideLoading();
            
        }

        async Task getMonitorInfo()
        {
            try
            {
                if (CurrentMonitor.Name != "")
                {
                    NameInfo.Text = CurrentMonitor.Name;
                }
                else
                {
                    await Task.Delay(500);
                    QueryName();
                }

                if (CurrentMonitor.RN != "")
                {
                    RNInfo.Text = CurrentMonitor.RN;
                }
                else
                {
                    await Task.Delay(500);
                    QueryRN();
                }

                if (CurrentMonitor.SN != "")
                {
                    SNInfo.Text = CurrentMonitor.SN;
                }
                else
                {
                    await Task.Delay(500);
                    QuerySN();
                }

                if (CurrentMonitor.Firmware != "")
                {
                    FirmwareInfo.Text = CurrentMonitor.Firmware;
                }
                else
                {
                    await Task.Delay(500);
                    QueryFirmware();
                }

                if (CurrentMonitor.Temperature != "")
                {
                    TempInfo.Text = CurrentMonitor.Temperature;
                    TempInfo.Text += "°C";
                }
                else
                {
                    await Task.Delay(500);
                    QueryTemp();
                }

                if (CurrentMonitor.TimeOn != "")
                {
                    TimeInfo.Text = CurrentMonitor.TimeOn;
                    TimeInfo.Text += "h.";
                }
                else
                {
                    await Task.Delay(500);
                    QueryTime();
                }

            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Problem retrieving information. {0}", e));
            }
        }

        private bool CheckAvailabilityMonitor()
        {
            try
            {
                string result = QueryData(ISIC_SCP_IF.CMD_MAN);
                if (result != "Not Available")
                {
                    IsicDebug.DebugSerial(String.Format("Monitor responding MAN command: {0}", result));
                    return true;
                }
                else
                {
                    IsicDebug.DebugSerial(String.Format("Monitor NOT responding MAN command {0}", result));
                    return false;
                }
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Exception thrown when sending MAN command to monitor. {0}", e));
                return false;
            }

        }

        private void QueryName()
        {
            string name = QueryData(ISIC_SCP_IF.CMD_TYP);

            if (!String.IsNullOrEmpty(name))
            {
                NameInfo.Text = name;
                CurrentMonitor.Name = name;
            }
            else
            {
                NameInfo.Text = "Data not available";
            }

        }
        private void QueryRN()
        {
            string rn = QueryData(ISIC_SCP_IF.CMD_REF);

            if (!String.IsNullOrEmpty(rn) && Regex.IsMatch(rn, @"^\w{2}\d{2}\w{3}$"))
            {
                SNInfo.Text = rn;
                CurrentMonitor.RN = rn;
            }
            else
            {
                RNInfo.Text = "Data not available";
            }
        }

        private void QuerySN()
        {
            string sn = QueryData(ISIC_SCP_IF.CMD_SNB);

            if (!String.IsNullOrEmpty(sn) && Regex.IsMatch(sn, @"^\d{2}\w\d{7}$"))
            {
                SNInfo.Text = sn;
                CurrentMonitor.SN = sn;
            }
            else
            {
                SNInfo.Text = "Data not available";
            }
        }

        private void QueryFirmware()
        {
            string firmware = QueryData(ISIC_SCP_IF.CMD_VER);

            if (!String.IsNullOrEmpty(firmware) && (Regex.IsMatch(firmware, @"^\d{5}-\d{3}\w$") || Regex.IsMatch(firmware, @"^\d{5}-\d{3}-\w$")))
            {
                FirmwareInfo.Text = firmware;
                CurrentMonitor.Firmware = firmware;
            }
            else
            {
                FirmwareInfo.Text = "Data not available";
            }


        }

        private void QueryTemp()
        {
            byte[] data = { 0x52 };
            string temperature = QueryData(ISIC_SCP_IF.CMD_TMP, data);

            if (!String.IsNullOrEmpty(temperature) && Regex.IsMatch(temperature, @"^\w\d{3}$"))
            {
                temperature = temperature.Substring(1);
                TempInfo.Text = temperature;
                TempInfo.Text += "°C";
                CurrentMonitor.Temperature = temperature;
            } else
            {
                TempInfo.Text = "Data not available";
            }
          
        }

        private void QueryTime()
        {
            byte[] data = { 0x30 };
            string timeOn = QueryData(ISIC_SCP_IF.CMD_ETC, data);

            if (!String.IsNullOrEmpty(timeOn) && Regex.IsMatch(timeOn, @"^\d{6}$"))
            {
                TimeInfo.Text = timeOn;
                TimeInfo.Text += "h.";
                CurrentMonitor.TimeOn = timeOn;
            }
            else
            {
                TimeInfo.Text = "Data not available";
            }
        }

        private string QueryData(string command, params byte[] data)
        {
            byte[] receivedData = null;
            int tries = 10;
            int internalTries = 10;
            string result = null;

            Stopwatch sw = Stopwatch.StartNew();
            if (CurrentCharacteristic.CanRead)
            {
                try
                {
                    CurrentCharacteristic.StartUpdatesAsync();
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}, {1}", command, sw.ElapsedMilliseconds));
                    sw.Restart();
                    while (receivedData == null && tries-- > 0)
                    {
                        new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command, data).Send(CurrentCharacteristic);
                        IsicDebug.DebugSerial(String.Format("- Sent command: {0} TIME:{1}", command, sw.ElapsedMilliseconds));
                        sw.Restart();

                        Task.Delay(400);

                        do
                        {
                            receivedData = CurrentCharacteristic.Value;
                            IsicDebug.DebugSerial(String.Format("DATA: {0} - {1}", receivedData.GetHexString(), receivedData.GetString()));
                            Task.Delay(1);
                        }
                        while (receivedData[0] != 0x06 && --internalTries >= 0);

                        if (receivedData[0] == 0x06)
                        {
                            IsicDebug.DebugSerial(String.Format("Received Data: {0} - {1}", receivedData.GetHexString(), receivedData.GetString()));
                            result = RetrieveDataFromMonitor(receivedData);
                        }
                        else
                        {
                            receivedData = null;
                            IsicDebug.DebugSerial(String.Format("Data in CurrentCharacteristic: {0} - {1}", CurrentCharacteristic.Value.GetHexString(), CurrentCharacteristic.Value.GetString()));
                        }
                    
                    }
                }
                catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("Not able to send or receive input data.", ex));
                }
            }
            else
            {
                IsicDebug.DebugBluetooth("This characteristic cannot read data.");
            }
            return result;
        }

        private string RetrieveDataFromMonitor(byte[] rArr)
        {
            string data = null;
            string result = null;
            try
            {
                data = ISIC_SCP_IF.GetDataStringFromCmdReply(rArr).GetString();
                Array.Clear(CurrentCharacteristic.Value, 0, CurrentCharacteristic.Value.Length);
                if (rArr != null)
                {
                    if (data != null)
                    {
                        IsicDebug.DebugSerial(String.Format("Transformed Data to value: {0}", data));
                        result = data;
                    }
                }
                else
                {
                    IsicDebug.DebugSerial(String.Format("Not receiving any Input data from the monitor"));
                }
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Problem receiving data from monitor {0}", e));
            }
            return result;
        }

        bool activeBuzzer = false;
        byte buzzerData = new byte();
        private void Buzzer_Toggled(object sender, ToggledEventArgs e)
        {
            if (activeBuzzer == false)  //If it's false -> Turn buzzer on: 0xFF
            {
                activeBuzzer = true;
                buzzerData = 0xFF;
                QueryData(ISIC_SCP_IF.CMD_BZZ, buzzerData);
            }
            else
            {                           //It's true -> Turn buzzer off: 0x00
                activeBuzzer = false;
                buzzerData = 0x00;
                QueryData(ISIC_SCP_IF.CMD_BZZ, buzzerData);
            }
        }

        bool activeFan = false;
        byte fanData = new byte();
        private void Fan_Toggled(object sender, ToggledEventArgs e)
        {
            if (activeFan == false)  //If it's false -> Turn fan on: 0xFF
            {
                activeFan = true;
                fanData = 0xFF;
                QueryData(ISIC_SCP_IF.CMD_FAN, fanData);
            }
            else
            {                           //It's true -> Turn fan off: 0x00
                activeFan = false;
                fanData = 0x00;
                QueryData(ISIC_SCP_IF.CMD_FAN, fanData);
            }
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

    }

}
