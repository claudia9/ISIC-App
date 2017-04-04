using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//Monitors Enumeration
public enum MonitorIdentifier
{
    Monitor1,
    Monitor2,
    Monitor3,
    MonitorBroadcast
}

namespace ISIC_FMT_MMCP_App
{
    public class MonitorSettings
    { 
        public byte MonAddr { get; set; }
        public int ToD { get; set; }
        public int ToDBacklightValue { get; set; }

        public string Name { get; set; }
        public string RN { get; set; }
        public string SN { get; set; }
        public string Firmware { get; set; }
        public string Temperature { get; set; }
        public string TimeOn { get; set; }

        public MonitorSettings()
        {
            MonAddr = 0x00;
            ToD = 0;
            ToDBacklightValue = 0;

            Name = "";
            RN = "";
            SN = "";
            Firmware = "";
            Temperature = "";
            TimeOn = "";
        }
    }
}
