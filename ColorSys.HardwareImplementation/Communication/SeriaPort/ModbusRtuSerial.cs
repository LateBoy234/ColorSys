using ColorSys.HardwareContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class ModbusRtuSerial : ICommunication
    {
        public string ConnectionId => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public Task ConnectAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> SendAsync(byte[] data, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

    }
}
