using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract
{
    public  interface IDevice:IMeasureMent, IDisposable
    {
        string InstrumentId { get; }
        string Model { get; }
        ICommunication Comm { get; }
    }
}
