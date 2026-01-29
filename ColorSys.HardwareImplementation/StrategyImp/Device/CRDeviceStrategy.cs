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
    public class CRDeviceStrategy : IDeviceStrategy
    {
        public string DisplayName => "CR色差宝";

        public DeviceType DeviceType => DeviceType.CR;

        public IDevice GetDevice(ICommunication comm)
        {
            return new  CRInstrument(comm);
        }
    }
}
