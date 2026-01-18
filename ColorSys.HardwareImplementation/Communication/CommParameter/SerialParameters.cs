using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.CommParameter
{
    public  class SerialParameters : ICommParameters
    {
        public string PortName { get; set; } = "";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public ParityBit Parity { get; set; } = ParityBit.None;
        public StopBit StopBits { get; set; } = StopBit.One;
    }

    public sealed class GetSerialParaRequest : RequestMessage<SerialParameters> { }
}
