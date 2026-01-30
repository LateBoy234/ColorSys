using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class BluetoothComm : ICommunication
    {
        private readonly BlutoothParameter _p;
       
        public BluetoothComm(BlutoothParameter p) => _p = p;


        public bool IsConnected => throw new NotImplementedException();

        public bool SupportsPlugDetect => throw new NotImplementedException();

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public event EventHandler<byte[]> DataReceived;

        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReconnectAsync()
        {
            throw new NotImplementedException();
        }

      

        public Task SendAsync(byte[] frame)
        {
            throw new NotImplementedException();
        }
    }
}
