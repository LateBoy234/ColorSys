using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.Strategy
{
    /// <summary>
    /// 设备策略
    /// </summary>
    public  interface IDeviceStrategy
    {
        string DisplayName {  get; }
        DeviceType DeviceType { get; }

        IDevice GetDevice(ICommunication comm);

    }
}
