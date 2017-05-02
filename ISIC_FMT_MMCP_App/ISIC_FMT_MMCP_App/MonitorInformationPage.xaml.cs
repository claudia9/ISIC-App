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

        IDescriptor CurrentDescriptor;

        LoadingViewModel loadingViewModel;

        bool valueUpdated = false;


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
            InitializeBle();

            InitializeMonitorInfo();
        }

        private void InitializeBle()
        {
            CurrentCharacteristic.ValueUpdated += CurrentCharacteristic_ValueUpdated;
        }

        private async void InitializeScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            this.CurrentDescriptor = await CurrentCharacteristic.GetDescriptorAsync(Guid.Parse("00002902-0000-1000-8000-00805f9b34fb"));
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
            await Task.Delay(1500);
            await getMonitorInfo();
            UserDialogs.Instance.HideLoading();

        }

        async Task getMonitorInfo()
        {
            //if (CheckAvailabilityMonitor())
            if (true)
            {
                try
                {
                    if (CurrentMonitor.Name != "")
                    {
                        NameInfo.Text = CurrentMonitor.Name;
                    }
                    else
                    {
                        QueryName();
                    }

                    if (CurrentMonitor.RN != "")
                    {
                        RNInfo.Text = CurrentMonitor.RN;
                    }
                    else
                    {
                        await Task.Delay(5);
                        QueryRN();
                    }

                    if (CurrentMonitor.SN != "")
                    {
                        SNInfo.Text = CurrentMonitor.SN;
                    }
                    else
                    {
                        await Task.Delay(5);
                        QuerySN();
                    }

                    if (CurrentMonitor.Firmware != "")
                    {
                        FirmwareInfo.Text = CurrentMonitor.Firmware;
                    }
                    else
                    {
                        await Task.Delay(5);
                        QueryFirmware();
                    }

                    if (CurrentMonitor.Temperature != "")
                    {
                        TempInfo.Text = CurrentMonitor.Temperature;
                        TempInfo.Text += "°C";
                    }
                    else
                    {
                        await Task.Delay(5);
                        QueryTemp();
                    }

                    if (CurrentMonitor.TimeOn != "")
                    {
                        TimeInfo.Text = CurrentMonitor.TimeOn;
                        TimeInfo.Text += "h.";
                    }
                    else
                    {
                        await Task.Delay(5);
                        QueryTime();
                    }

                }
                catch (Exception e)
                {
                    IsicDebug.DebugException(String.Format("Problem retrieving information. {0}", e));
                }
            }
            else
            {
                UserDialogs.Instance.Alert("There is no connection with the Monitor - No data is retriable.");
            }
        }

        //NOT IN USE
        private bool CheckAvailabilityMonitor()
        {
            bool result = false;
            try
            {
                int i = 10;
                while (i-- > 0 && result == false)
                {
                    result = QuerySingleData(ISIC_SCP_IF.CMD_MAN);
                }
                if (result != false)
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
            string name = null;
            //await Task.Factory.StartNew(() =>
            //{
            name = QueryData(ISIC_SCP_IF.CMD_TYP);
            //});

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
            string rn = null;
                rn = QueryData(ISIC_SCP_IF.CMD_REF);

            if (!String.IsNullOrEmpty(rn) && Regex.IsMatch(rn, @"^\w{2}\d{2}\w{3}$"))
            {
                RNInfo.Text = rn;
                CurrentMonitor.RN = rn;
            }
            else
            {
                RNInfo.Text = "Data not available";
            }

        }

        private void QuerySN()
        {
            string sn = null;
            
                sn = QueryData(ISIC_SCP_IF.CMD_SNB);

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
            string firmware = null;

            firmware = QueryData(ISIC_SCP_IF.CMD_VER);

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
            string temperature = null;

            temperature = QueryData(ISIC_SCP_IF.CMD_TMP, data);
            

            if (!String.IsNullOrEmpty(temperature) && Regex.IsMatch(temperature, @"^\w\d{3}$"))
            {
                temperature = temperature.Substring(1);
                TempInfo.Text = temperature;
                TempInfo.Text += "°C";
                CurrentMonitor.Temperature = temperature;
            }
            else
            {
                TempInfo.Text = "Data not available";
            }

        }

        private void QueryTime()
        {
            byte[] data = { 0x30 };
            string timeOn = null;

            timeOn = QueryData(ISIC_SCP_IF.CMD_ETC, data);


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

        private bool QuerySingleData(string command)
        {
            bool success = false;

            if (CurrentCharacteristic.CanWrite)
            {
                Task.Delay(1000);
                try
                {
                    valueUpdated = false;
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));
                    Task.Delay(1000);

                    int tries = 10;

                    CurrentCharacteristic.StartUpdatesAsync();
                    Task.Delay(1000);
                    new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command).Send(CurrentCharacteristic);
                    Task.Delay(1000);

                    while (tries-- > 0)
                    {
                        CurrentCharacteristic.ReadAsync();
                        if (valueUpdated == true)
                        {
                            IsicDebug.DebugSerial(String.Format("Received something from monitor: {0}", CurrentCharacteristic.Value.GetHexString()));
                            success = true;
                            break;
                        }
                        else
                        {
                            IsicDebug.DebugSerial("Not able to read data.");
                        }

                    }
                    CurrentCharacteristic.StopUpdatesAsync();
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
            return success;
        }


        private void CurrentCharacteristic_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            if (CurrentCharacteristic.Value[0] == 0x06)
            {
                valueUpdated = true;
            }
            else
            {
                valueUpdated = false;
            }
        }

        private string QueryData(string command, params byte[] data)
        {
            byte[] receivedData = null;
            int tries = 50;
            int internaltries = 5;
            string result = null;
            valueUpdated = false;


            CurrentCharacteristic.StartUpdatesAsync();
            if (CurrentCharacteristic.CanWrite)
            {
                Task.Delay(5);
                try
                {


                    while (tries-- > 0)
                    {
                        IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));
                        new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command, data).Send(CurrentCharacteristic);
                        Task.Delay(500);
                        CurrentCharacteristic.ReadAsync();
                        if (valueUpdated == true)
                        {
                            if (CurrentCharacteristic.Value[0] == 0x06)
                            {
                                receivedData = CurrentCharacteristic.Value;
                                result = RetrieveDataFromMonitor(receivedData);
                                IsicDebug.DebugSerial(String.Format("Received Data: {0} - {1}", receivedData.GetHexString(), receivedData.GetString()));
                                break;
                            }
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

        private async Task<byte[]> readData()
        {
            byte[] receivedData = null;
            if (CurrentCharacteristic.CanRead)
            {
                int tries = 50;
                while (CurrentCharacteristic.Value[0] != 0x06 && tries-- > 0)
                {
                    receivedData = await CurrentCharacteristic.ReadAsync();
                }
            }
            return receivedData;
        }

        private string RetrieveDataFromMonitor(byte[] rArr)
        {
            string data = null;
            string result = null;
            try
            {
                data = ISIC_SCP_IF.GetDataStringFromCmdReply(rArr).GetString();
                //Array.Clear(CurrentCharacteristic.Value, 0, CurrentCharacteristic.Value.Length);
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
                QueryBuzzer(ISIC_SCP_IF.CMD_BZZ, buzzerData);
                //QueryData(ISIC_SCP_IF.CMD_BZZ, buzzerData);
            }
            else
            {                           //It's true -> Turn buzzer off: 0x00
                activeBuzzer = false;
                buzzerData = 0x00;
                QueryBuzzer(ISIC_SCP_IF.CMD_BZZ, buzzerData);
                //QueryData(ISIC_SCP_IF.CMD_BZZ, buzzerData);
            }
        }
        private void QueryBuzzer(string command, params byte[] data)
        {

            if (CurrentCharacteristic.CanRead)
            {
                try
                {
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));

                    new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command, data).Send(CurrentCharacteristic);
                    Task.Delay(20);
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

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();

            return true;
        }

    }

}
