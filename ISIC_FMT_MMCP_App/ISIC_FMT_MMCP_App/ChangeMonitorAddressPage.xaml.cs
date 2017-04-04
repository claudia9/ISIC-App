using Acr.UserDialogs;
using Isic.Debugger;
using Plugin.BLE.Abstractions.Contracts;
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
        private byte PreviousAddress;
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
        }

        private void InitializeComboboxes()
        {
            for (int i = 0; i < 254; i++)
            {
                Monitors.Items.Add("Monitor at address " + i.ToString());
            }

            for (int i = 0; i < 254; i++)
            {
                Addresses.Items.Add(i.ToString());
            }
        }
        private void InitializeButtons()
        {
            UnlockLabel.IsVisible = false;
            Monitors.SelectedIndexChanged += Monitors_SelectedIndexChanged;
            Lock.Toggled += Lock_Toggled;
        }



        private void Monitors_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreviousAddress = (byte)((sender as Picker).SelectedIndex);
        }

        private void Lock_Toggled(object sender, ToggledEventArgs e)
        {
            if (locked == false)
            {
                locked = true;
                UserDialogs.Instance.Toast(String.Format("Monitor at address: {0} is LOCKED", PreviousAddress));
                LockMonitor();
            }
            else
            {
                locked = false;
                UserDialogs.Instance.Toast(String.Format("Monitor at address: {0} is UNLOCKED", PreviousAddress));
                UnLockMonitor();
            }

        }

        private void UnLockMonitor()
        {
            throw new NotImplementedException();
        }

        private void LockMonitor()
        {

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
