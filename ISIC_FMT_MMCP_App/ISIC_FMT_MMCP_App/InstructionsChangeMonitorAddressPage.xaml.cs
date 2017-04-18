using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class InstructionsChangeMonitorAddressPage : Rg.Plugins.Popup.Pages.PopupPage
    {
        public InstructionsChangeMonitorAddressPage()
        {
            InitializeScreen();
            InitializeComponent();
            InitializeButtons();
        }

        private void InitializeScreen()
        {
            this.BackgroundColor = new Color(0, 0, 0, 0.4);
        }

        private void InitializeButtons()
        {
            Close.Clicked += Close_Clicked;
        }

        private void Close_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.PopAsync();
        }
    }
}
