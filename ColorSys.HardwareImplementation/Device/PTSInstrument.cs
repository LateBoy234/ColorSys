using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public  class PTSInstrument : IDevice
    {
        

        private readonly ICommunication _comm;

        public PTSInstrument(ICommunication comm)   // Autofac 自动注入
        {
            _comm = comm;
        }
        public DeviceType DeviceType => DeviceType.PTS;

        public bool IsConnected => _comm.IsConnected;
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
            Comm.Dispose();
        }

        public async Task<TestModel> RunTestAsync(CancellationToken token = default)
        {
            Comm.SendAsync(new byte[] { 0x55, 0xaa, 0xa1, 0x00, 0x00, 0x00, 0x02, 0x00, 0x02 });
            return await Task.Delay(1000).ContinueWith(x => new TestModel());
           // return await Comm.ReceiveAsync<TestModel>(token);
        }
    }
}
