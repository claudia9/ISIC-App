using Acr.UserDialogs;
using Isic.Debugger;
using Isic.SerialProtocol;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorInformationPage : ContentPage
    {
        private MonitorSettings CurrentMonitor;
        ICharacteristic CurrentCharacteristic;
        public MonitorInformationPage(MonitorSettings monitor, ICharacteristic characteristic)
        {
            this.CurrentMonitor = monitor;
            this.CurrentCharacteristic = characteristic;
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

            CurrentCharacteristic.ValueUpdated += ReceivedData;

            Buzzer.Toggled += Buzzer_Toggled;
        }

        private void InitializeMonitorInfo()
        {
            if (CheckAvailabilityMonitor() == true)
            {
                try
                {
                    QueryName();
                    QueryRN();
                    QuerySN();
                    QueryFirmware();
                    QueryBaud();
                    QueryTemp();
                    QueryTime();
                }
                catch (Exception e)
                {
                    IsicDebug.DebugException(String.Format("Problem retrieving information. {0}", e));
                }
            }
            else
            {
                UserDialogs.Instance.Toast("The connection with the monitor has been lost. Information cannot be retrieved.");
            }

        }

        private bool CheckAvailabilityMonitor()
        {
            try
            {
                string result = QueryData(ISIC_SCP_IF.CMD_MAN);
                if (result != null)
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

            NameInfo.Text = name;
        }
        private void QueryRN()
        {
            string rn = QueryData(ISIC_SCP_IF.CMD_REF);
            /*while (rn == "Not available" || rn == null)
            {
                rn = QueryData(ISIC_SCP_IF.CMD_REF);
            }*/
            RNInfo.Text = rn;
        }

        private void QuerySN()
        {
            string sn = QueryData(ISIC_SCP_IF.CMD_SNB);
            SNInfo.Text = sn;
        }

        private void QueryFirmware()
        {
            string firmware = QueryData(ISIC_SCP_IF.CMD_VER);
            FirmwareInfo.Text = firmware;
        }

        private void QueryBaud()
        {
            string baud = QueryData(ISIC_SCP_IF.CMD_EBR);   //Not sure about the command!!
            BaudInfo.Text = baud;
        }

        private void QueryTemp()
        {
            string temperature = QueryData(ISIC_SCP_IF.CMD_TMP);
            TempInfo.Text = temperature;
        }

        private void QueryTime()
        {
            string timeOn = QueryData(ISIC_SCP_IF.CMD_ETC);
            TimeInfo.Text = timeOn;
        }

        private void Buzzer_Toggled(object sender, ToggledEventArgs e)
        {
            UserDialogs.Instance.Toast("Buzzer has been toggled.");
        }

        private bool data_received = false;
        private string QueryData(string command)
        {
            IsicDebug.DebugSerial(String.Format("Received Data: {0}", CurrentCharacteristic.Value.GetHexString()));
            if (CurrentCharacteristic.CanRead)
            {
                try
                {
                    CurrentCharacteristic.StartUpdatesAsync();
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));
                    int tries = 0;
                    while (data_received != true && tries < 5)
                    {
                        new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command).Send(CurrentCharacteristic);
                        IsicDebug.DebugSerial(String.Format("Received Data: {0}", CurrentCharacteristic.Value.GetHexString()));

                        tries++;
                    }
                    if (data_received == true)
                    {
                        return RetrieveDataFromMonitor();
                    }
                    return "Not Available";
                }
                catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("Not able to send or receive input data.", ex));
                    return "Not Available";
                }
            }
            else
            {
                IsicDebug.DebugBluetooth("This characteristic cannot read data.");
                return "Not Available";
            }
        }

        private void ReceivedData(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            if (e.Characteristic.Value[0] == 0x06)
            {
                data_received = true;
            }
            else
            {
                data_received = false;
            }
            //CurrentCharacteristic.StopUpdatesAsync();
        }

        private string RetrieveDataFromMonitor()
        {
            data_received = false;
            string data = null;
            try
            {
                byte[] rArr = CurrentCharacteristic.Value;
                data = ISIC_SCP_IF.GetDataStringFromCmdReply(rArr).GetString();
                if (rArr != null)
                {
                    IsicDebug.DebugSerial(String.Format("Received Data: {0}", rArr.GetHexString()));
                    if (data != null)
                    {
                        IsicDebug.DebugSerial(String.Format("Transformed Data to value: {0}", data));
                        return data;
                    }
                    else
                    {
                        return "SHIT";
                    }
                }
                else
                {
                    IsicDebug.DebugSerial(String.Format("Not receiving any Input data from the monitor"));
                    return "Not Available";
                }
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Problem receiving data from monitor {0}", e));
                return "Not Available";
            }
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

    }

}
