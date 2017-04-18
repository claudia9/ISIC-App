using Acr.UserDialogs;
using Isic.Debugger;
using Isic.SerialProtocol;
using Plugin.BLE.Abstractions.Contracts;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class ChangeMonitorAddressPage : ContentPage
    {
        private byte AfterAddress;

        private bool locked;

        private ICharacteristic CurrentCharacteristic;

        public ChangeMonitorAddressPage(ICharacteristic characteristic)
        {
            CurrentCharacteristic = characteristic;
            InitializeScreen();
            InitializeComponent();
            InitializeComboboxes();
            InitializeButtons();
        }
        private void InitializeScreen()
        {

            NavigationPage.SetHasNavigationBar(this, false);
            if ((bool)Application.Current.Properties["Show_Instructions_MonAddr"] == false)
            {
                Navigation.PushPopupAsync(new InstructionsChangeMonitorAddressPage());
            } else
            {

                UserDialogs.Instance.Alert("Please, turn off all the monitors except the one willing to have another address.", null, "Done");
            }


        }

        private void InitializeComboboxes()
        {
            for (int i = 0; i < 254; i++)
            {
                Addresses.Items.Add(i.ToString());
            }
        }
        private void InitializeButtons()
        {
            HelpButton.Clicked += HelpButton_Clicked;

            SaveButton.Clicked += SaveButton_Clicked;
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            ChangeAddress();
        }

        private void ChangeAddress()
        {
            string command = ISIC_SCP_IF.CMD_SDA;
            byte[] data = Addresses.SelectedIndex.ToString().GetBytes();
            SendCommand(command, data);
            
        }

        private void SendCommand(string command, byte[] data)
        {
            try
                {
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));

                    new Isic.SerialProtocol.Command(0xFF, command, data).Send(CurrentCharacteristic);
                    Task.Delay(20);
                }

                catch (Exception ex)
                {
                    IsicDebug.DebugException(String.Format("Not able to send or receive input data.", ex));
                }
        }

        private void HelpButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushPopupAsync(new InstructionsChangeMonitorAddressPage());
        }



        /*private string QueryData(string command, params byte[] data)
        {
            byte[] receivedData = null;
            if (CurrentCharacteristic.CanRead)
            {
                try
                {
                    CurrentCharacteristic.StartUpdatesAsync();
                    IsicDebug.DebugSerial(String.Format("Sending.....{0}", command));
                    int tries = 0;
                    while (receivedData == null && tries < 10)
                    {
                        new Isic.SerialProtocol.Command(CurrentMonitor.MonAddr, command, data).Send(CurrentCharacteristic);
                        receivedData = CurrentCharacteristic.Value;
                        if (receivedData[0] == 0x06)
                        {
                            IsicDebug.DebugSerial(String.Format("Received Data: {0} - {1}", receivedData.GetHexString(), receivedData.GetString()));
                            return RetrieveDataFromMonitor(receivedData);
                        }
                        else
                        {
                            receivedData = null;
                            IsicDebug.DebugSerial(String.Format("Data in CurrentCharacteristic: {0} - {1}", CurrentCharacteristic.Value.GetHexString(), CurrentCharacteristic.Value.GetString()));
                        }

                        tries++;
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
    }*/
    }
}
