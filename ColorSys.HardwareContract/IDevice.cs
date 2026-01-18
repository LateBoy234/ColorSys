using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract
{
    public  interface IDevice:IMeasureMent, IDisposable
    {
        DeviceType DeviceType { get; } // "PTS" / "CR"
        Task<bool> ConnectAsync(); // 内部调用 ICommunication
        bool IsConnected { get; }
        ICommunication Comm { get; }
    }
}
