using Isic.Debugger;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class RoundedMonitorButton : Button
    {
        public bool IsClicked { get; set; }

        //public List<RoundedMonitorButton> MonitorButtonsArray;


        public RoundedMonitorButton()
        {
            IsClicked = false;
            /*MonitorButtonsArray.Add(this);
            for (int i = 0; i < MonitorButtonsArray.Count; i++) {
                IsicDebug.DebugGeneral("Array of Monitor buttons: " + MonitorButtonsArray[i].Text);
            }*/
        }
    }
}
