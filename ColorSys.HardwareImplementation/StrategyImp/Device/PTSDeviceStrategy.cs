using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.StrategyImp.Device
{
    public class PTSDeviceStrategy : IDeviceStrategy
    {
        public string DisplayName => "PTS系列";

        public DeviceType DeviceType => DeviceType.PTS;

        public IDevice GetDevice(ICommunication comm)
        {
            return new PTSInstrument(comm);
        }
    }
}
