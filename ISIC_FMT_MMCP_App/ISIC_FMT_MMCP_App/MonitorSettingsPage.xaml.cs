using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class MonitorSettingsPage : ContentPage
    {

        private Dictionary<MonitorIdentifier, MonitorSettings> monitors { get; set; }
        private Dictionary<int, byte> AddressDictionary = new Dictionary<int, byte>
        {
            { 0, 0x00 }, {1, 0x01 }, {2, 0x02 }, {3, 0x03 }, {4, 0x04 }, {5, 0x05 }, {6, 0x06 }, {255, 0xFF }
        };

        public MonitorSettingsPage()
        {
            InitializeComponent();
            InitializeDictionary();
            InitializeComboBoxes();

            Commit.Clicked += Commit_Clicked;

            Debug.WriteLine("On MonitorSettingsPage");
        }

        private void Commit_Clicked(object sender, EventArgs e)
        {
            setMonitor1Address((byte)Mon1Addr.SelectedIndex);
            setMonitor2Address((byte)Mon2Addr.SelectedIndex);
            setMonitor3Address((byte)Mon3Addr.SelectedIndex);
        }

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

        private void InitializeDictionary()
        {
            monitors = new Dictionary<MonitorIdentifier, MonitorSettings>();
            monitors[MonitorIdentifier.Monitor1] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor2] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor3] = new MonitorSettings();
            monitors[MonitorIdentifier.Monitor4] = new MonitorSettings();
            monitors[MonitorIdentifier.MonitorBroadcast] = new MonitorSettings() { MonAddr = 0xFF };
        }

        private void InitializeComboBoxes()
        {
            Picker mon1 = Mon1Addr;
            foreach (int address in AddressDictionary.Keys)
            {
                mon1.Items.Add(address.ToString());
            }

            Picker mon2 = Mon2Addr;
            foreach (int address in AddressDictionary.Keys)
            {
                mon2.Items.Add(address.ToString());
            }


            Picker mon3 = Mon3Addr;
            foreach (int address in AddressDictionary.Keys)
            {
                mon3.Items.Add(address.ToString());
            }
        }
    }
}
