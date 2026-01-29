using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public class CRInstrument : IDevice
    {
        private readonly ICommunication _comm;
        public CRInstrument(ICommunication communication)
        {
            _comm = communication;
        }
        public DeviceType DeviceType => DeviceType.CR;

        public bool IsConnected => Comm.IsConnected;

        public ICommunication Comm => _comm;

       

        public async Task<bool> ConnectAsync()
        {
            await _comm.ConnectAsync();
            if (!_comm.IsConnected)
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            Comm.Dispose(); ;
        }

        public async Task<TestModel> RunTestAsync(CancellationToken token = default)
        {
            var request = new byte[] { 0x55, 0xaa, 0xa1, 0x00, 0x00, 0x00, 0x02, 0x00, 0x02 };
            var response = await _comm.SendAndReceiveAsync(request, timeoutMs: 3000, token);
            return await Task.Delay(1000).ContinueWith(x => new TestModel());
        }
    }
}
