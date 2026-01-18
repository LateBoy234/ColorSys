using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract
{
    public interface IConnectedDeviceHub : INotifyPropertyChanged
    {
        IDevice? Current { get; set; }
    }
}
