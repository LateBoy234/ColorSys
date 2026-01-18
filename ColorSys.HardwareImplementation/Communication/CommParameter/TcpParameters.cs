using ColorSys.HardwareContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.CommParameter
{
    public  class TcpParameters:ICommParameters
    {
        public string IP { get; set; } = "192.168.1.100";
        public int Port { get; set; } = 502;
    }
}
