using ColorSys.HardwareContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.CommParameter
{
    public  class BlutoothParameter:ICommParameters
    {
        public string MacAddress { get; set; } = "";
    }
}
