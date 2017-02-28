using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISIC_FMT_MMCP_App
{
    public class MonitorSettings
    {

        public byte MonAddr { get; set; }
        public int ToD { get; set; }
        public int ToDBacklightValue { get; set; }

        public MonitorSettings()
        {
            MonAddr = 0xFF;
            ToD = -1;
            ToDBacklightValue = -1;
        }
    }
}
