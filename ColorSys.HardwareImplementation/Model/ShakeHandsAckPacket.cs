using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Model
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ShakeHandsAckPacket
    {
       
        int version; // 通信接口版本信息


        public int Version
        {
            get { return version & 0xff; }

            set { version = (version & ~0xff) | (0xff & value); }
        }

        public int ProductType
        {
            get { return (version >> 8) & 0xFFFFFF; }
            set { version = (version & 0xFF) | (value << 8); }
        }
    }
}
