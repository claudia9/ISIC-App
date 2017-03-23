using Isic.Debugger;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class RoundedMonitorButton : Button
    {
        public bool IsClicked { get; set; }


        public RoundedMonitorButton()
        {
            IsClicked = false;
        }
    }
}
